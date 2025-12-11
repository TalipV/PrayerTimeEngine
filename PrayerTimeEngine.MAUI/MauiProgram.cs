using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using MetroLog.MicrosoftExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plugin.Maui.DebugRainbows;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Data.EntityFramework.Generated_CompiledModels;
using PrayerTimeEngine.Core.Data.WebSocket;
using PrayerTimeEngine.Core.Data.WebSocket.Interfaces;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Services;
using PrayerTimeEngine.Core.Domain.ConfigurationManagement;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.IslamicCalendar.Interfaces;
using PrayerTimeEngine.Core.Domain.IslamicCalendar.Services;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Services;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Services;
using PrayerTimeEngine.Presentation.Pages.DatabaseTables;
using PrayerTimeEngine.Presentation.Pages.Main;
using PrayerTimeEngine.Presentation.Pages.Settings.SettingsContent;
using PrayerTimeEngine.Presentation.Pages.Settings.SettingsContent.Custom;
using PrayerTimeEngine.Presentation.Pages.Settings.SettingsHandler;
using PrayerTimeEngine.Presentation.Services;
using PrayerTimeEngine.Presentation.Services.Navigation;
using PrayerTimeEngine.Presentation.Services.SettingsContentPageFactory;
using PrayerTimeEngine.Presentation.Views;
using PrayerTimeEngine.Presentation.Views.MosquePrayerTimes;
using PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic;
using PrayerTimeEngine.Presentation.Views.PrayerTimes;
using PrayerTimeEngine.Services;
using PrayerTimeEngine.Services.Notifications;
using Refit;
using System.Net.WebSockets;
using System.Text;
using UraniumUI;

namespace PrayerTimeEngine;

// weak event manager?


/* CLI commands cheat sheet (executed in solution folder)
 * Generate Release APK:    dotnet publish -c release -f net10.0-android -p:false
 * EF Migration:            dotnet ef migrations add InitialMigration --output-dir Data\EntityFramework\Generated_Migrations --namespace PrayerTimeEngine.Core.Data.EntityFramework.Generated_Migrations --context PrayerTimeEngine.Core.Data.EntityFramework.AppDbContext --project PrayerTimeEngine.Core\PrayerTimeEngine.Core.csproj
 * EF Compiled Models:      dotnet ef dbcontext optimize --output-dir Data\EntityFramework\Generated_CompiledModels --namespace PrayerTimeEngine.Core.Data.EntityFramework.Generated_CompiledModels --context PrayerTimeEngine.Core.Data.EntityFramework.AppDbContext --project PrayerTimeEngine.Core\PrayerTimeEngine.Core.csproj
 */

/* BUGS:
 * - Next Fajr (value and for graphic) & Last 'Isha (for graphic)
 * - No robust system for country and city (re)load from fazilet/semerkand API (e.g. failed city retrieval leads to no second try)
 * - Fazilet/Semerkand country/city names which are unexpected (e.g. "Vienna (Wien)")
 * - Turkish location details for Fazilet/Semerkand not robust
 * - Semerkand sometimes puts "*" in the json for their times which is not handled in the app (e.g. "*23:54")
 * - Semerkand sometimes has countries and cities with duplicate names
 * 
 * - REMOVE bypassed SSL validation for ISemerkandApiService
 * - Exception for single calculation source failing to provide location info prevents rest
 * - Exception for single calculation prevents all the other calculations (rethink this and make tests)
 * - Exception for single calculation only disables that calculation but subsequent calculations rely on cached values and don't retry
 * - remove AppDbContextModel _useOldBehavior31751 temp fix
 * - FIXED? Calculation relevant data like the Profile with its configs may change in the middle of the calculation process due to shared references
 * - MyMosq: "DhuhrTIME" is earlier than "Dhuhr" but "Fajr" is earlier than "FajrTIME". Bug?? One of them has to be the time for the congregation/iqama and the other one the actual prayer time
 * - Fazilet & Semerkand place search should also try german country names and/or city names
 */

/* TODO general:
 * - Autostart of app on device startup (like for example the Fazilet app does)
 * - For prayer times of a time zone other than that of the device show option to select based on which time zone to show the times (like a yes/no slider)
 * - After importing profiles, show overview of current and new profiles for the user to select which ones to keep
 * - Reconsider IDailyPrayerTimes properties ('Date' as a ZonedDateTime?? Why carry DateTimeZone info redundantely?)
 * - Decrease count of reloads (e.g. mere app switch shouldn't always require reload)
 * - Transactions when saving country data and city data to prevent partial safes (or rethink the whole thing)
 * - PlaceService and ProfileService with no or default CancellationToken?
 * - Consider Muwaqqit API changes and maybe additionally use old API endpoint
 * - Consistent naming of awaitable methods with or without Async suffix
 * - Also check navigation properties back and forth in Equals override? Mixed approaches currently
 * - Change "DBAccess" to "Repository" and make sure the code there really is only about db access / repository
 * - Check for possibly unsafe concurrent actions (fast user interactions, app crashes and other special cases) 
 * - Make MyMosqApiService more robust and try to read inputs as jsons
 * - Logging
 * - Comments
 * - Translation
 * - Check MVVM
 * - CancellationTokens implementieren, die default setzen
 * - "MosquePrayerTimes" and variations used for all kinds of things! Better names!
 * - Check if using multiple profiles from differing timezones works fine (graphic, mosque times, swiping back and forth, ...)
 */

/* TODO tests:
 * ### UNIT
 * # Semerkand
 * --- SemerkandDynamicPrayerTimeProvider
 * --- SemerkandApiService
 * --- SemerkandDBAccess
 * # Fazilet
 * --- FaziletDynamicPrayerTimeProvider
 * --- FaziletApiService
 * --- FaziletDBAccess
 * # Muwaqqit
 * --- MuwaqqitDynamicPrayerTimeProvider
 * --- MuwaqqitApiService
 * --- MuwaqqitDBAccess
 * # DynamicPrayerTimeProviderManager
 * # MyMosqPrayerTimes & MawaqitPrayerTimes: One test each to validate their respective scraped JSON inputs (i.e. every time text is a valid time and so on)
 * # ProfileService: One test for UpdateLocationInfo which considers the different Fazilet/Semerkand place info custom things
 */

public static class MauiProgram
{
    public static readonly DateTime StartDateTime = DateTime.Now;
    public static IServiceProvider ServiceProvider { get; private set; }

    public static bool IsFullyInitialized { get; set; } = false;

    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkitMarkup()
            .UseMauiCommunityToolkit(options =>
            {
                options.SetShouldEnableSnackbarOnWindows(true);
            })
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .UseDebugRainbows(new DebugRainbowsOptions { ShowRainbows = false })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        addLogging(builder);
        addDependencyInjectionServices(builder.Services);

        MauiApp mauiApp = builder.Build();
        ServiceProvider = mauiApp.Services;

        MethodTimeLogger.Logger = mauiApp.Services.GetRequiredService<ILogger<App>>();

        return mauiApp;
    }

    class LoggingLayout : MetroLog.Layouts.Layout
    {
        public override string GetFormattedString(MetroLog.LogWriteContext context, MetroLog.LogEventInfo info)
        {
            var text = new StringBuilder($"███ {info.Level}|{info.TimeStamp:HH:mm:ss:fff}|{info.Logger}|{info.Message}");

            if (info.Exception is not null)
            {
                text.Append($"|Exception:'{info.Exception.Message}' AT '{info.Exception.StackTrace}'");
            }

            //text.Append($"|StackTrace: {new StackTrace()}");

            return text.ToString();
        }
    }

    private static void addLogging(MauiAppBuilder builder)
    {
        builder.Logging
            .SetMinimumLevel(LogLevel.Trace)
            .AddFilter((loggerProviderFullName, loggerFullName, level) =>
            {
                // temp fix, these log too much for my taste
                if (loggerFullName.StartsWith("Microsoft.EntityFrameworkCore")
                    || loggerFullName.StartsWith("System.Net.Http.HttpClient.Refit.Implementation.Generated")
                    || loggerFullName == "Polly")
                {
                    return false;
                }

                return true;
            })
            // for the logs to be visible within the app
            .AddInMemoryLogger(
                options =>
                {
                    //options.MaxLines = 1024;
                    options.Layout = new LoggingLayout();
                    //options.MinLevel = LogLevel.Trace;
                    //options.MaxLevel = LogLevel.Error;
                })
            // for the logs to be sharable as files through the UI
            .AddStreamingFileLogger(
                options =>
                {
                    options.RetainDays = 2;
                    //options.MinLevel = LogLevel.Trace;
                    //options.MaxLevel = LogLevel.Error;
                    options.FolderPath = Path.Combine(
                        FileSystem.CacheDirectory,
                        "MetroLogs");
                });
    }

    private const int HTTP_REQUEST_TIMEOUT_SECONDS = 40;

    private static void addDependencyInjectionServices(IServiceCollection serviceCollection)
    {
        // Note: Microsoft recommends explicit HttpClient instances without DI for MAUI

        serviceCollection.AddDbContextFactory<AppDbContext>(options =>
        {
            options.UseModel(AppDbContextModel.Instance);
            options.UseSqlite($"Data Source={AppConfig.DATABASE_PATH}",
                sqlLiteConfig => sqlLiteConfig.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));

            //options.ConfigureWarnings(x => x.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
            //options.LogTo(Console.WriteLine, LogLevel.Trace);
        },
        lifetime: ServiceLifetime.Singleton);

        serviceCollection.AddSingleton<AppDbContextMetaData>();

        serviceCollection.AddTransient<IWebSocketClientFactory, WebSocketClientFactory>();
        serviceCollection.AddTransient<IWebSocketClient, WebSocketClient>(factory =>
        {
            return new WebSocketClient(new ClientWebSocket());
        });

        serviceCollection.AddTransient<NotificationService>();
        serviceCollection.AddSingleton<ISystemInfoService, SystemInfoService>();
        serviceCollection.AddTransient<IPreferenceService, PreferenceService>();
        serviceCollection.AddTransient<IIslamicDateCalculationService, IslamicDateCalculationService>();
        serviceCollection.AddTransient<IConfigurationImportExportService, ConfigurationImportExportService>();
        serviceCollection.AddSingleton<TimeTypeAttributeService>();

        serviceCollection.AddTransient<IDynamicPrayerTimeProviderManager, DynamicPrayerTimeProviderManager>();
        serviceCollection.AddTransient<IDynamicPrayerTimeProviderFactory, DynamicPrayerTimeProviderFactory>();

        serviceCollection.AddTransient<IMosquePrayerTimeProviderManager, MosquePrayerTimeProviderManager>();
        serviceCollection.AddTransient<IMosquePrayerTimeProviderFactory, MosquePrayerTimeProviderFactory>();

        serviceCollection.AddTransient<IProfileService, ProfileService>();
        serviceCollection.AddTransient<IProfileDBAccess, ProfileDBAccess>();

        serviceCollection.AddTransient<IPlaceService, PlaceService>(sp =>
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://eu1.locationiq.com/v1/"),
                Timeout = TimeSpan.FromSeconds(HTTP_REQUEST_TIMEOUT_SECONDS)
            };

            return new PlaceService(
                RestService.For<ILocationIQApiService>(httpClient),
                sp.GetRequiredService<ISystemInfoService>(),
                sp.GetRequiredService<ILogger<PlaceService>>());
        });

        addCalculatorServices(serviceCollection);
        addPresentationLayerServices(serviceCollection);
        addPlatformSpecificServices(serviceCollection);
    }

    private static void addCalculatorServices(IServiceCollection serviceCollection)
    {
        // FAZILET
        serviceCollection.AddTransient<IFaziletDBAccess, FaziletDBAccess>();
        serviceCollection.AddTransient<IPrayerTimeCacheCleaner, FaziletDBAccess>();
        serviceCollection
            .AddRefitClient<IFaziletApiService>()
            .ConfigureHttpClient(config =>
            {
                config.Timeout = TimeSpan.FromSeconds(HTTP_REQUEST_TIMEOUT_SECONDS);
                config.BaseAddress = new Uri("https://fazilettakvimi.com/api/cms/");
            })
            .AddStandardResilienceHandler();
        serviceCollection.AddTransient<FaziletDynamicPrayerTimeProvider>();

        // SEMERKAND
        serviceCollection.AddTransient<ISemerkandDBAccess, SemerkandDBAccess>();
        serviceCollection.AddTransient<IPrayerTimeCacheCleaner, SemerkandDBAccess>();
        serviceCollection
            .AddRefitClient<ISemerkandApiService>()
            .ConfigureHttpClient(config =>
            {
                config.Timeout = TimeSpan.FromSeconds(HTTP_REQUEST_TIMEOUT_SECONDS);
                config.BaseAddress = new Uri("https://semerkandtakvimi.semerkandmobile.com/");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // TODO: REMOVE, because Semerkand will hopefully fix this soon
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            })
            .AddStandardResilienceHandler();
        serviceCollection.AddTransient<SemerkandDynamicPrayerTimeProvider>();

        // MUWAQQIT
        serviceCollection.AddTransient<IMuwaqqitDBAccess, MuwaqqitDBAccess>();
        serviceCollection.AddTransient<IPrayerTimeCacheCleaner, MuwaqqitDBAccess>();
        serviceCollection
            .AddRefitClient<IMuwaqqitApiService>()
            .ConfigureHttpClient(config =>
            {
                config.Timeout = TimeSpan.FromSeconds(HTTP_REQUEST_TIMEOUT_SECONDS);
                config.BaseAddress = new Uri("https://www.muwaqqit.com/");
            })
            .AddStandardResilienceHandler();
        serviceCollection.AddTransient<MuwaqqitDynamicPrayerTimeProvider>();

        // MYMOSQ
        serviceCollection.AddTransient<IMyMosqDBAccess, MyMosqDBAccess>();
        serviceCollection.AddTransient<IPrayerTimeCacheCleaner, MyMosqDBAccess>();
        serviceCollection.AddTransient<IMyMosqApiService, MyMosqApiService>();
        serviceCollection.AddTransient<MyMosqMosquePrayerTimeProvider>();

        // MAWAQIT
        serviceCollection.AddTransient<IMawaqitDBAccess, MawaqitDBAccess>();
        serviceCollection.AddTransient<IPrayerTimeCacheCleaner, MawaqitDBAccess>();
        serviceCollection.AddTransient<IMawaqitApiService, MawaqitApiService>(factory =>
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(HTTP_REQUEST_TIMEOUT_SECONDS),
                BaseAddress = new Uri("https://mawaqit.net/de/")
            };
            return new MawaqitApiService(httpClient);
        });
        serviceCollection.AddTransient<MawaqitMosquePrayerTimeProvider>();
    }

    private static void addPresentationLayerServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IBrowser>(factory => Browser.Default);

        serviceCollection.AddTransient<INavigationService, NavigationService>();
        serviceCollection.AddTransient<ToastMessageService>();

        serviceCollection.AddTransient<MainPage>();
        serviceCollection.AddTransient<MainPageViewModel>();
        serviceCollection.AddTransient<PrayerTimeGraphicView>();

        serviceCollection.AddTransient<PrayerTimeViewModelFactory>();
        serviceCollection.AddTransient<DynamicPrayerTimeViewModel>();
        serviceCollection.AddTransient<MosquePrayerTimeViewModel>();

        serviceCollection.AddTransient<SettingsHandlerPage>();
        serviceCollection.AddTransient<SettingsHandlerPageViewModel>();
        serviceCollection.AddTransient<SettingsContentPageFactory>();

        serviceCollection.AddTransient<SettingsContentPage>();
        serviceCollection.AddTransient<SettingsContentPageViewModel>();
        serviceCollection.AddTransient<MuwaqqitDegreeSettingConfigurationViewModel>();

        serviceCollection.AddTransient<DatabaseTablesPage>();
        serviceCollection.AddTransient<DatabaseTablesPageViewModel>();
    }

    private static void addPlatformSpecificServices(IServiceCollection serviceCollection)
    {
        var prayerTimeSummaryNotificationHandlerMock = NSubstitute.Substitute.For<IPrayerTimeSummaryNotificationHandler>();
        NSubstitute.SubstituteExtensions.Returns(prayerTimeSummaryNotificationHandlerMock.ExecuteAsync(), Task.CompletedTask);

#if ANDROID
        serviceCollection.AddTransient<IPrayerTimeSummaryNotificationHandler, Platforms.Android.Notifications.PrayerTimeSummaryNotificationHandler>();
#else
        // TODO implement for other platforms someday (at least iOS)
        serviceCollection.AddTransient<IPrayerTimeSummaryNotificationHandler>(factory => prayerTimeSummaryNotificationHandlerMock);
#endif
    }
}
