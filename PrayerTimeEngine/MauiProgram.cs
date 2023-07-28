using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Code.Domain;
using PrayerTimeEngine.Code.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Code.Domain.Calculator.Fazilet.Interfaces;
using PrayerTimeEngine.Code.Domain.Calculator.Fazilet.Services;
using PrayerTimeEngine.Code.Domain.Calculator.Muwaqqit.Interfaces;
using PrayerTimeEngine.Code.Domain.Calculator.Muwaqqit.Services;
using PrayerTimeEngine.Code.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Code.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Code.Domain.ConfigStore.Interfaces;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PrayerTimeEngine.Code.Domain.ConfigStore.Services;
using PrayerTimeEngine.Code.Presentation.Service.Navigation;
using PrayerTimeEngine.Code.Presentation.Service.SettingsContentPageFactory;
using PrayerTimeEngine.Code.Presentation.View;
using PrayerTimeEngine.Code.Presentation.ViewModel;
using PrayerTimeEngine.Code.Presentation.ViewModel.Custom;
using PrayerTimeEngine.Views;

namespace PrayerTimeEngine;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        addDependencyInjectionServices(builder.Services);

        return builder.Build();
    }

    private static void addDependencyInjectionServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ISQLiteDB, SQLiteDB>();
        serviceCollection.AddSingleton<IPrayerTimeCalculationService, PrayerTimeCalculationService>();
        serviceCollection.AddSingleton<TimeTypeAttributeService>();

        serviceCollection.AddTransient<FaziletPrayerTimeCalculator>();
        serviceCollection.AddSingleton<IFaziletDBAccess, FaziletDBAccess>();
        serviceCollection.AddHttpClient<IFaziletApiService, FaziletApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
            client.BaseAddress = new Uri("https://fazilettakvimi.com/api/cms/");
        });

        serviceCollection.AddTransient<SemerkandPrayerTimeCalculator>();
        serviceCollection.AddSingleton<ISemerkandDBAccess, SemerkandDBAccess>();
        serviceCollection.AddHttpClient<ISemerkandApiService, SemerkandApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        serviceCollection.AddTransient<MuwaqqitPrayerTimeCalculator>();
        serviceCollection.AddSingleton<IMuwaqqitDBAccess, MuwaqqitDBAccess>();
        serviceCollection.AddHttpClient<IMuwaqqitApiService, MuwaqqitApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        serviceCollection.AddTransient<IConfigStoreService, ConfigStoreService>();
        serviceCollection.AddTransient<IConfigStoreDBAccess, ConfigStoreDBAccess>();

        serviceCollection.AddSingleton<PrayerTimesConfigurationStorage>();

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
