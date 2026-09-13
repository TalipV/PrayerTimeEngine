using System.Net.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Data.EntityFramework.Generated_CompiledModels;
using PrayerTimeEngine.Core.Data.WebSocket;
using PrayerTimeEngine.Core.Data.WebSocket.Interfaces;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Services;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Services;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Services;
using Refit;

namespace PrayerTimeEngine.Core.Infrastructure;

/// <summary>
/// Composition of ONION ring 3 (Infrastructure): the concrete adapters that implement the
/// Domain ports — EF Core persistence, Refit HTTP clients, WebSocket and the place service.
/// The inner rings never reference these types; only this method wires them up.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    private const int HTTP_REQUEST_TIMEOUT_SECONDS = 40;
    private const int RESILIENCE_ATTEMPT_TIMEOUT_SECONDS = 20;
    private const int RESILIENCE_TOTAL_REQUEST_TIMEOUT_SECONDS = 35;

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string databasePath,
        string locationIQApiKey)
    {
        addPersistence(services, databasePath);
        addWebSocket(services);
        addPlaceService(services, locationIQApiKey);
        addDynamicPrayerTimeProviders(services);
        addMosquePrayerTimeProviders(services);
        return services;
    }

    private static void addPersistence(IServiceCollection services, string databasePath)
    {
        services.AddDbContextFactory<AppDbContext>(options =>
        {
            // Necessary in AppDbContextModel because otherwise EF migrations deadlock on app startup
            AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue31751", true);

            options.UseModel(AppDbContextModel.Instance);
            options.UseSqlite($"Data Source={databasePath}",
                sqlLiteConfig => sqlLiteConfig.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        },
        lifetime: ServiceLifetime.Singleton);

        services.AddSingleton<AppDbContextMetaData>();

        services.AddTransient<IProfileRepository, ProfileRepository>();
    }

    private static void addWebSocket(IServiceCollection services)
    {
        services.AddTransient<IWebSocketClientFactory, WebSocketClientFactory>();
        services.AddTransient<IWebSocketClient, WebSocketClient>(_ => new WebSocketClient(new ClientWebSocket()));
    }

    private static void addPlaceService(IServiceCollection services, string locationIQApiKey)
    {
        services.AddTransient<IPlaceService, PlaceService>(sp =>
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://eu1.locationiq.com/v1/"),
                Timeout = TimeSpan.FromSeconds(HTTP_REQUEST_TIMEOUT_SECONDS)
            };

            return new PlaceService(
                RestService.For<ILocationIQApiService>(httpClient),
                sp.GetRequiredService<ISystemInfoService>(),
                sp.GetRequiredService<ILogger<PlaceService>>(),
                locationIQApiKey);
        });
    }

    private static void addDynamicPrayerTimeProviders(IServiceCollection services)
    {
        // FAZILET
        services.AddTransient<IFaziletRepository, FaziletRepository>();
        services.AddTransient<IPrayerTimeCacheCleaner, FaziletRepository>();
        addResilientRefitClient<IFaziletApiClient>(services, "https://fazilettakvimi.com/api/cms/");
        services.AddTransient<IFaziletApiService, FaziletApiService>();

        // SEMERKAND
        services.AddTransient<ISemerkandRepository, SemerkandRepository>();
        services.AddTransient<IPrayerTimeCacheCleaner, SemerkandRepository>();
        addResilientRefitClient<ISemerkandApiClient>(services, "https://semerkandtakvimi.com/api/");
        services.AddTransient<ISemerkandApiService, SemerkandApiService>();

        // MUWAQQIT
        services.AddTransient<IMuwaqqitRepository, MuwaqqitRepository>();
        services.AddTransient<IPrayerTimeCacheCleaner, MuwaqqitRepository>();
        addResilientRefitClient<IMuwaqqitApiClient>(services, "https://www.muwaqqit.com/");
        services.AddTransient<IMuwaqqitApiService, MuwaqqitApiService>();
    }

    private static void addMosquePrayerTimeProviders(IServiceCollection services)
    {
        // MYMOSQ
        services.AddTransient<IMyMosqRepository, MyMosqRepository>();
        services.AddTransient<IPrayerTimeCacheCleaner, MyMosqRepository>();
        services.AddTransient<IMyMosqApiService, MyMosqApiService>();

        // MAWAQIT
        services.AddTransient<IMawaqitRepository, MawaqitRepository>();
        services.AddTransient<IPrayerTimeCacheCleaner, MawaqitRepository>();
        services.AddTransient<IMawaqitApiService, MawaqitApiService>(_ =>
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(HTTP_REQUEST_TIMEOUT_SECONDS),
                BaseAddress = new Uri("https://mawaqit.net/de/")
            };
            return new MawaqitApiService(httpClient);
        });
    }

    private static void addResilientRefitClient<TClient>(IServiceCollection services, string baseAddress)
        where TClient : class
    {
        IHttpClientBuilder httpClientBuilder = services
            .AddRefitGeneratedClient<TClient>()
            .ConfigureHttpClient(config =>
            {
                config.Timeout = TimeSpan.FromSeconds(HTTP_REQUEST_TIMEOUT_SECONDS);
                config.BaseAddress = new Uri(baseAddress);
            });

        httpClientBuilder.AddStandardResilienceHandler(options =>
        {
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(RESILIENCE_ATTEMPT_TIMEOUT_SECONDS);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(RESILIENCE_TOTAL_REQUEST_TIMEOUT_SECONDS);

            // validation requires it to be at least twice the attempt timeout
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(RESILIENCE_ATTEMPT_TIMEOUT_SECONDS * 2);
        });
    }
}
