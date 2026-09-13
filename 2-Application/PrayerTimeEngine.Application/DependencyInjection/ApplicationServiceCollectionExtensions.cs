using Microsoft.Extensions.DependencyInjection;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.ConfigurationManagement;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.IslamicCalendar.Interfaces;
using PrayerTimeEngine.Core.Domain.IslamicCalendar.Services;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Services;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Services;

namespace PrayerTimeEngine.Core.Application;

/// <summary>
/// Composition of ONION ring 2 (Application): use-case orchestration.
/// Registers only inner-ring services; the concrete adapters they depend on
/// (repositories, API clients, DbContext) come from <c>AddInfrastructure</c>.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // domain / application services
        services.AddSingleton<TimeTypeAttributeService>();
        services.AddTransient<IIslamicDateCalculationService, IslamicDateCalculationService>();
        services.AddTransient<IConfigurationImportExportService, ConfigurationImportExportService>();

        services.AddTransient<IProfileService, ProfileService>();
        services.AddSingleton<IProfileVersionStore, ProfileVersionStore>();

        // dynamic prayer times
        services.AddSingleton<IDynamicPrayerTimeProviderManager, DynamicPrayerTimeProviderManager>();
        services.AddTransient<IDynamicPrayerTimeProviderFactory, DynamicPrayerTimeProviderFactory>();
        services.AddTransient<FaziletDynamicPrayerTimeProvider>();
        services.AddTransient<SemerkandDynamicPrayerTimeProvider>();
        services.AddTransient<MuwaqqitDynamicPrayerTimeProvider>();

        // mosque prayer times
        services.AddTransient<IMosquePrayerTimeProviderManager, MosquePrayerTimeProviderManager>();
        services.AddTransient<IMosquePrayerTimeProviderFactory, MosquePrayerTimeProviderFactory>();
        services.AddTransient<MyMosqMosquePrayerTimeProvider>();
        services.AddTransient<MawaqitMosquePrayerTimeProvider>();

        return services;
    }
}
