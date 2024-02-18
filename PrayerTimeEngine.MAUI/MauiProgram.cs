using CommunityToolkit.Maui;
using MetroLog.MicrosoftExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Code.Presentation.View;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.CalculationManagement;
using PrayerTimeEngine.Core.Domain.Calculators;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Services;
using PrayerTimeEngine.Presentation.Service.Navigation;
using PrayerTimeEngine.Presentation.Service.SettingsContentPageFactory;
using PrayerTimeEngine.Presentation.ViewModel;
using PrayerTimeEngine.Presentation.ViewModel.Custom;
using PrayerTimeEngine.Services.SystemInfoService;
using PrayerTimeEngine.Views;
using UraniumUI;

namespace PrayerTimeEngine;

// dotnet publish -c release -f net7.0-android -p:false

// BUGS:
// - Next Fajr (value and for graphic) & Last 'Isha (for graphic)
// - No robust system for country and city (re)load from fazilet/semerkand API (e.g. failed city retrieval leads to no second try)
// - Fazilet/Semerkand country/city names which are unexpected (e.g. "Vienna (Wien)")
// - Turkish location details for Fazilet/Semerkand not robust
// - Exception for single calculation prevents all the other calculations

// TODO:
// - Performance
// - Multiple profiles
// - Decrease count of reloads (e.g. mere app switch shouldn't always require reload)
// - CancellationTokens

// TODO late:
// - Check for possibly unsafe concurrent actions (fast user interactions, app crashes and other special cases) 
// - Logging
// - Comments
// - Translation
// - Check MVVM

public static class MauiProgram
{
    public static readonly DateTime StartDateTime = DateTime.Now;
    public static IServiceProvider ServiceProvider { get; private set; }

    public static bool IsFullyInitialized = false;

    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        addLogging(builder);
        addDependencyInjectionServices(builder.Services);

        MauiApp mauiApp = builder.Build();
        ServiceProvider = mauiApp.Services;

        MethodTimeLogger.logger = mauiApp.Services.GetService<ILogger<App>>();

        return mauiApp;
    }

    class LoggingLayout : MetroLog.Layouts.Layout
    {
        public override string GetFormattedString(MetroLog.LogWriteContext context, MetroLog.LogEventInfo info)
        {
            string text = $"███ {info.Level}|{info.TimeStamp:HH:mm:ss:fff}|{info.Logger}|{info.Message}";

            if (info.Exception != null)
            {
                text += $"|Exception:'{info.Exception.Message}' AT '{info.Exception.StackTrace}'";
            }

            return text;
        }
    }

    private static void addLogging(MauiAppBuilder builder)
    {
        builder.Logging
            .SetMinimumLevel(LogLevel.Trace)
            .AddFilter((loggerProviderFullName, loggerFullName, level) =>
            {
                // temp fix, EF logs too much about its queries
                if (loggerFullName.StartsWith("Microsoft.EntityFrameworkCore"))
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

    private static void addDependencyInjectionServices(IServiceCollection serviceCollection)
    {
        // Assumptions:
        // - Singleton instead of transient if problems with something like states or thread safety are not expected, and disposing is not necessary
        // - Microsoft recommends explicit HttpClient instances without DI for MAUI

        serviceCollection.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite($"Data Source={AppConfig.DATABASE_PATH}", x => x.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            options.UseModel(AppDbContextModel.Instance);

            //options.ConfigureWarnings(x => x.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
            //options.LogTo(Console.WriteLine, LogLevel.Trace);
        },
        contextLifetime: ServiceLifetime.Transient,
        optionsLifetime: ServiceLifetime.Singleton);

        serviceCollection.AddSingleton<ISystemInfoService, SystemInfoService>();
        serviceCollection.AddSingleton<ICalculationManager, CalculationManager>();
        serviceCollection.AddSingleton<IPrayerTimeServiceFactory, PrayerTimeServiceFactory>();
        serviceCollection.AddSingleton<TimeTypeAttributeService>();

        serviceCollection.AddSingleton<IPlaceService, PlaceService>(sp =>
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
            return new PlaceService(httpClient, sp.GetService<ILogger<PlaceService>>());
        });

        serviceCollection.AddSingleton<IProfileService, ProfileService>();
        serviceCollection.AddSingleton<IProfileDBAccess, ProfileDBAccess>();

        //serviceCollection.AddSingleton<PreferenceService>();
        //serviceCollection.AddSingleton<IPreferenceAccess, PreferenceAccess>();

#if ANDROID
        serviceCollection.AddSingleton<Services.PrayerTimeSummaryNotificationManager>();
#endif

        addApiServices(serviceCollection);
        addPresentationLayerServices(serviceCollection);
    }

    private static void addApiServices(IServiceCollection serviceCollection)
    {
        // FAZILET
        serviceCollection.AddSingleton<FaziletPrayerTimeCalculator>();
        serviceCollection.AddSingleton<IFaziletDBAccess, FaziletDBAccess>();
        serviceCollection.AddSingleton<IFaziletApiService, FaziletApiService>(sp =>
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20),
                BaseAddress = new Uri("https://fazilettakvimi.com/api/cms/")
            };
            return new FaziletApiService(httpClient);
        });

        // SEMERKAND
        serviceCollection.AddSingleton<SemerkandPrayerTimeCalculator>();
        serviceCollection.AddSingleton<ISemerkandDBAccess, SemerkandDBAccess>();
        serviceCollection.AddSingleton<ISemerkandApiService, SemerkandApiService>(sp =>
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
            return new SemerkandApiService(httpClient);
        });

        // MUWAQQIT
        serviceCollection.AddSingleton<MuwaqqitPrayerTimeCalculator>();
        serviceCollection.AddSingleton<IMuwaqqitDBAccess, MuwaqqitDBAccess>();
        serviceCollection.AddSingleton<IMuwaqqitApiService, MuwaqqitApiService>(sp =>
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
            return new MuwaqqitApiService(httpClient);
        });
    }

    private static void addPresentationLayerServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<INavigationService, NavigationService>();

        serviceCollection.AddSingleton<MainPage>();
        serviceCollection.AddSingleton<MainPageViewModel>();

        serviceCollection.AddTransient<SettingsHandlerPage>();
        serviceCollection.AddTransient<SettingsHandlerPageViewModel>();
        serviceCollection.AddSingleton<SettingsContentPageFactory>();

        serviceCollection.AddTransient<SettingsContentPage>();
        serviceCollection.AddTransient<SettingsContentPageViewModel>();
        serviceCollection.AddTransient<MuwaqqitDegreeSettingConfigurationViewModel>();

        serviceCollection.AddTransient<DatabaseTablesPage>();
        serviceCollection.AddTransient<DatabaseTablesPageViewModel>();
    }
}
