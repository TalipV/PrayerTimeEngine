﻿using CommunityToolkit.Maui;
using DevExpress.Maui;
using MetroLog.MicrosoftExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Code.Presentation.View;
using PrayerTimeEngine.Core.Data.EntityFramework;
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

        addLogging(builder);
        addDependencyInjectionServices(builder.Services);

        return builder.Build();
    }

    class LoggingLayout : MetroLog.Layouts.Layout
    {
        public override string GetFormattedString(MetroLog.LogWriteContext context, MetroLog.LogEventInfo info)
        {
            // temp fix, EF logs too much about its queries
            if (info.Message.Contains("SELECT"))
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
            .AddInMemoryLogger(
                options =>
                {
                    //options.MaxLines = 1024;
                    options.Layout = new LoggingLayout();
                });
    }

    private static void addDependencyInjectionServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContext<AppDbContext>(options =>
        {
            string _databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "PrayerTimeEngineDB_ET.db");
            options.UseSqlite($"Data Source={_databasePath}", x => x.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            //options.ConfigureWarnings(x => x.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
            //options.LogTo(Console.WriteLine, LogLevel.Trace);
        }, ServiceLifetime.Transient);

        serviceCollection.AddSingleton<IPrayerTimeCalculationService, PrayerTimeCalculationService>();
        serviceCollection.AddSingleton<TimeTypeAttributeService>();
        serviceCollection.AddSingleton<ConcurrentDataLoader>();

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

        serviceCollection.AddHttpClient<ILocationService, LocationService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        serviceCollection.AddTransient<IProfileService, ProfileService>();
        serviceCollection.AddTransient<IProfileDBAccess, ProfileDBAccess>();

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
        serviceCollection.AddSingleton<SettingsContentPageFactory>();

        serviceCollection.AddTransient<MuwaqqitDegreeSettingConfigurationViewModel>();
    }
}
