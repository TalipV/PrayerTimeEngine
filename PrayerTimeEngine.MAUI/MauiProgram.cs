using CommunityToolkit.Maui;
using DevExpress.Maui;
using MethodTimer;
using MetroLog.MicrosoftExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Code.Presentation.View;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Data.Preferences;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Services;
using PrayerTimeEngine.Core.Domain.PlacesService.Interfaces;
using PrayerTimeEngine.Core.Domain.PlacesService.Services;
using PrayerTimeEngine.Presentation.Service.Navigation;
using PrayerTimeEngine.Presentation.Service.SettingsContentPageFactory;
using PrayerTimeEngine.Presentation.ViewModel;
using PrayerTimeEngine.Presentation.ViewModel.Custom;
using PrayerTimeEngine.Views;

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

    [Time]
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseDevExpress()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        addLogging(builder);
        addDependencyInjectionServices(builder.Services);

        MauiApp mauiApp = builder.Build();

        mauiApp.Services.GetService<ConcurrentDataLoader>().InitiateConcurrentDataLoad();
        MethodTimeLogger.logger = mauiApp.Services.GetService<ILogger<App>>();

        // slightly slowls down startup
        // DevExpress.Maui.Editors.Initializer.Init();
        // DevExpress.Maui.Controls.Initializer.Init();

        return mauiApp;
    }

    class LoggingLayout : MetroLog.Layouts.Layout
    {
        public override string GetFormattedString(MetroLog.LogWriteContext context, MetroLog.LogEventInfo info)
        {
            // temp fix, EF logs too much about its queries
            if (info.Message.Contains("DbCommand"))
            {
                return "";
            }

            string text = $"███ {info.TimeStamp:HH:mm:ss:fff}|{info.Level}|{info.Logger}|{info.Message}";

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
                if (loggerFullName == "Microsoft.EntityFrameworkCore.Model")
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
                })
            // for the logs to be sharable as files through the UI
            .AddStreamingFileLogger(
                options =>
                {
                    options.RetainDays = 2;
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
            string _databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrayerTimeEngineDB_ET.db");
            options.UseSqlite($"Data Source={_databasePath}", x => x.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            //options.ConfigureWarnings(x => x.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
            //options.LogTo(Console.WriteLine, LogLevel.Trace);
        }, ServiceLifetime.Transient);

        serviceCollection.AddSingleton<IPrayerTimeCalculationService, PrayerTimeCalculationService>();
        serviceCollection.AddSingleton<TimeTypeAttributeService>();
        serviceCollection.AddSingleton<ConcurrentDataLoader>();

        #region FaziletAPI

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

        #endregion FaziletAPI

        #region SemerkandAPI

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

        #endregion SemerkandAPI

        #region MuwaqqitAPI

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

        #endregion MuwaqqitAPI

        serviceCollection.AddSingleton<ILocationService, LocationService>(sp =>
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
            return new LocationService(httpClient, sp.GetService<ILogger<LocationService>>());
        });

        serviceCollection.AddSingleton<IProfileService, ProfileService>();
        serviceCollection.AddSingleton<IProfileDBAccess, ProfileDBAccess>();

        serviceCollection.AddSingleton<PreferenceService>();
        serviceCollection.AddSingleton<IPreferenceAccess, PreferenceAccess>();

        addPresentationLayerServices(serviceCollection);
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
    }
}
