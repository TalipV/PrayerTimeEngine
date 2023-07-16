using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Code.Domain.Fazilet.Interfaces;
using PrayerTimeEngine.Code.Domain.Fazilet.Services;
using PrayerTimeEngine.Code.Domain.Muwaqqit.Interfaces;
using PrayerTimeEngine.Code.Domain.Muwaqqit.Services;
using PrayerTimeEngine.Code.Services;
using PrayerTimeEngine.Presentation.ViewModels;

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

        serviceCollection.AddSingleton<MainPage>();
        serviceCollection.AddSingleton<MainPageViewModel>();
    }
}
