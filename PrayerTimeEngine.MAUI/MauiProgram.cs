using CommunityToolkit.Maui;
using DevExpress.Maui;
using MetroLog.MicrosoftExtensions;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Code.Presentation.View;
using PrayerTimeEngine.Domain;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Domain.ConfigStore.Interfaces;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.ConfigStore.Services;
using PrayerTimeEngine.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Domain.Configuration.Services;
using PrayerTimeEngine.Domain.NominatimLocation.Interfaces;
using PrayerTimeEngine.Presentation.Service.Navigation;
using PrayerTimeEngine.Presentation.Service.SettingsContentPageFactory;
using PrayerTimeEngine.Presentation.ViewModel;
using PrayerTimeEngine.Presentation.ViewModel.Custom;
using PrayerTimeEngine.Views;

namespace PrayerTimeEngine;

// dotnet publish -c release -f net7.0-android -p:false

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseDevExpress()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging
            .SetMinimumLevel(LogLevel.Debug) // IMPORTANT: set your minimum log level, here Trace
            .AddTraceLogger(
                options =>
                {
                    options.MinLevel = LogLevel.Debug;
                    options.MaxLevel = LogLevel.Debug;
                }) // Will write to the Debug Output
            .AddConsoleLogger(
                options =>
                {
                    options.MinLevel = LogLevel.Debug;
                    options.MaxLevel = LogLevel.Debug;
                }) // Will write to the Console Output (logcat for android)
            .AddInMemoryLogger(
                options =>
                {
                    options.MaxLines = 1024;
                    options.MinLevel = LogLevel.Debug;
                    options.MaxLevel = LogLevel.Debug;
                })
            .AddStreamingFileLogger(
                options =>
                {
                    options.RetainDays = 2;
                    options.FolderPath = Path.Combine(
                        FileSystem.CacheDirectory,
                        "MetroLogs");
                })
            .AddInMemoryLogger(_ => { })
            .AddStreamingFileLogger(_ => { });
#endif

        addDependencyInjectionServices(builder.Services);

        return builder.Build();
    }

    private static void addDependencyInjectionServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ISQLiteDB, SQLiteDB>();
        serviceCollection.AddSingleton<IPrayerTimeCalculationService, PrayerTimeCalculationService>();
        serviceCollection.AddSingleton<TimeTypeAttributeService>();

        #region FaziletAPI

        serviceCollection.AddTransient<FaziletPrayerTimeCalculator>();
        serviceCollection.AddSingleton<IFaziletDBAccess, FaziletDBAccess>();
        serviceCollection.AddHttpClient<IFaziletApiService, FaziletApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
            client.BaseAddress = new Uri("https://fazilettakvimi.com/api/cms/");
        });

        #endregion FaziletAPI

        #region SemerkandAPI

        serviceCollection.AddTransient<SemerkandPrayerTimeCalculator>();
        serviceCollection.AddSingleton<ISemerkandDBAccess, SemerkandDBAccess>();
        serviceCollection.AddHttpClient<ISemerkandApiService, SemerkandApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        #endregion SemerkandAPI

        #region MuwaqqitAPI

        serviceCollection.AddTransient<MuwaqqitPrayerTimeCalculator>();
        serviceCollection.AddSingleton<IMuwaqqitDBAccess, MuwaqqitDBAccess>();
        serviceCollection.AddHttpClient<IMuwaqqitApiService, MuwaqqitApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        #endregion MuwaqqitAPI

        serviceCollection.AddHttpClient<IPlaceService, PlaceService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        serviceCollection.AddTransient<IConfigStoreService, ConfigStoreService>();
        serviceCollection.AddTransient<IConfigStoreDBAccess, ConfigStoreDBAccess>();

        serviceCollection.AddSingleton<PrayerTimesConfigurationStorage>();
        serviceCollection.AddSingleton<IConfigurationSerializationService, ConfigurationSerializationService>();

        addPresentationLayerServices(serviceCollection);
    }

    private static void addPresentationLayerServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<INavigationService, NavigationService>();

        serviceCollection.AddTransient<MainPage>();
        serviceCollection.AddTransient<MainPageViewModel>();

        serviceCollection.AddTransient<SettingsHandlerPage>();
        serviceCollection.AddTransient<SettingsHandlerPageViewModel>();

        serviceCollection.AddTransient<SettingsContentPage>();
        serviceCollection.AddTransient<SettingsContentPageViewModel>();
        serviceCollection.AddSingleton<ISettingsContentPageFactory, SettingsContentPageFactory>();

        serviceCollection.AddTransient<MuwaqqitDegreeSettingConfigurationViewModel>();
    }
}
