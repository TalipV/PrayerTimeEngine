using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Code.Domain;
using PrayerTimeEngine.Code.Domain.ConfigStore.Interfaces;
using PrayerTimeEngine.Code.Domain.ConfigStore.Services;
using PrayerTimeEngine.Code.Domain.Fazilet.Interfaces;
using PrayerTimeEngine.Code.Domain.Fazilet.Services;
using PrayerTimeEngine.Code.Domain.Muwaqqit.Interfaces;
using PrayerTimeEngine.Code.Domain.Muwaqqit.Services;
using PrayerTimeEngine.Code.Interfaces;
using PrayerTimeEngine.Code.Presentation.Service.Navigation;
using PrayerTimeEngine.Code.Presentation.Service.SettingsContentPageFactory;
using PrayerTimeEngine.Code.Presentation.View;
using PrayerTimeEngine.Code.Presentation.ViewModel;
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
        serviceCollection.AddSingleton<IPrayerTimeCalculatorFactory, PrayerTimeCalculatorFactory>();

        serviceCollection.AddTransient<FaziletPrayerTimeCalculator>();
        serviceCollection.AddSingleton<IFaziletDBAccess, FaziletDBAccess>();
        serviceCollection.AddSingleton<IFaziletApiService, FaziletApiService>();

        serviceCollection.AddTransient<MuwaqqitPrayerTimeCalculator>();
        serviceCollection.AddSingleton<IMuwaqqitDBAccess, MuwaqqitDBAccess>();
        serviceCollection.AddSingleton<IMuwaqqitApiService, MuwaqqitApiService>();

        serviceCollection.AddTransient<MainPage>();
        serviceCollection.AddTransient<MainPageViewModel>();

        serviceCollection.AddTransient<SettingsMainPage>();
        serviceCollection.AddTransient<SettingsMainPageViewModel>();

        serviceCollection.AddTransient<SettingsContentPage>();
        serviceCollection.AddTransient<SettingsContentPageViewModel>();
        serviceCollection.AddSingleton<ISettingsContentPageFactory, SettingsContentPageFactory>();

        serviceCollection.AddTransient<IConfigStoreService, ConfigStoreService>();
        serviceCollection.AddTransient<IConfigStoreDBAccess, ConfigStoreDBAccess>();

        serviceCollection.AddSingleton<PrayerTimesConfigurationStorage>();
        serviceCollection.AddTransient<INavigationService, NavigationService>();

    }
}
