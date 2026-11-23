using System.Net.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Core.Common.Http;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Data.EntityFramework.Generated_CompiledModels;
using PrayerTimeEngine.Core.Data.JsonSerialization;
using PrayerTimeEngine.Core.Data.WebSocket;
using PrayerTimeEngine.Core.Data.WebSocket.Interfaces;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Services;
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
        AddPersistence(services, databasePath);
        AddWebSocket(services);
        AddPlaceService(services, locationIQApiKey);
        AddDynamicPrayerTimeProviders(services);
        AddMosquePrayerTimeProviders(services);
        return services;
    }

    private static void AddPersistence(IServiceCollection services, string databasePath)
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
        services.AddTransient<IFavoritePlaceRepository, FavoritePlaceRepository>();
    }

    private static void AddWebSocket(IServiceCollection services)
    {
        services.AddTransient<IWebSocketClientFactory, WebSocketClientFactory>();
        services.AddTransient<IWebSocketClient, WebSocketClient>(_ => new WebSocketClient(new ClientWebSocket()));
    }

    private static void AddPlaceService(IServiceCollection services, string locationIQApiKey)
    {
        // Free plan: 2 requests per second (plus minute and day quotas). The search while typing and the
        // turkish name look ups of Fazilet/Semerkand right after a place selection easily hit that, so
        // LocationIQ gets the most careful throttle and the same retry pipeline as the other APIs.
        AddResilientRefitClient<ILocationIQApiService>(
            services,
            "https://eu1.locationiq.com/v1/",
            new ApiThrottleOptions(
                ApiName: "LocationIQ",
                MinRequestInterval: TimeSpan.FromMilliseconds(550),
                MaxRequestInterval: TimeSpan.FromSeconds(3),
                DefaultCooldown: TimeSpan.FromSeconds(1),
                MaxCooldown: TimeSpan.FromMinutes(5),
                MaxWaitBeforeFailing: s_maxThrottleWait));

        services.AddTransient<IPlaceService, PlaceService>(sp =>
            new PlaceService(
                sp.GetRequiredService<ILocationIQApiService>(),
                locationIQApiKey));
    }

    private static void AddDynamicPrayerTimeProviders(IServiceCollection services)
    {
        // FAZILET
        services.AddTransient<IFaziletRepository, FaziletRepository>();
        services.AddTransient<IPrayerTimeCacheCleaner, FaziletRepository>();
        // announces 100/s, 5000/min and 50000/h (RateLimit-* headers, measured 2026-11): practically unlimited
        AddResilientRefitClient<IFaziletApiClient>(services, "https://backend.fazilettakvimi.com/content/public/",
            CreateThrottleOptions("Fazilet", minRequestIntervalMs: 0));
        services.AddTransient<IFaziletApiService, FaziletApiService>();

        // SEMERKAND
        services.AddTransient<ISemerkandRepository, SemerkandRepository>();
        services.AddTransient<IPrayerTimeCacheCleaner, SemerkandRepository>();
        // no announced limit, 150 requests with 20 in parallel (~40/s) were all answered (measured 2026-11)
        AddResilientRefitClient<ISemerkandApiClient>(services, "https://semerkandtakvimi.com/api/",
            CreateThrottleOptions("Semerkand", minRequestIntervalMs: 50));
        services.AddTransient<ISemerkandApiService, SemerkandApiService>();

        // MUWAQQIT
        services.AddTransient<IMuwaqqitRepository, MuwaqqitRepository>();
        services.AddTransient<IPrayerTimeCacheCleaner, MuwaqqitRepository>();
        // no announced limit, 150 requests with 20 in parallel (~18/s) were all answered (measured 2026-11);
        // a small private service, so it is asked politely
        AddResilientRefitClient<IMuwaqqitApiClient>(services, "https://www.muwaqqit.com/",
            CreateThrottleOptions("Muwaqqit", minRequestIntervalMs: 100));
        services.AddTransient<IMuwaqqitApiService, MuwaqqitApiService>();

        // WAQTI
        services.AddTransient<IWaqtiRepository, WaqtiRepository>();
        services.AddTransient<IPrayerTimeCacheCleaner, WaqtiRepository>();
        // no announced limit (one request returns a whole month)
        AddResilientRefitClient<IWaqtiApiClient>(services, "https://www.waqti.app/",
            CreateThrottleOptions("Waqti", minRequestIntervalMs: 100));
        services.AddTransient<IWaqtiApiService, WaqtiApiService>();
    }

    private static void AddMosquePrayerTimeProviders(IServiceCollection services)
    {
        // MYMOSQ
        services.AddTransient<IMyMosqRepository, MyMosqRepository>();
        services.AddTransient<IPrayerTimeCacheCleaner, MyMosqRepository>();
        services.AddTransient<IMyMosqApiService, MyMosqApiService>();

        // MAWAQIT
        services.AddTransient<IMawaqitRepository, MawaqitRepository>();
        services.AddTransient<IPrayerTimeCacheCleaner, MawaqitRepository>();
        // HTML pages behind Cloudflare without an announced limit: asked politely
        IHttpClientBuilder mawaqitClientBuilder = services
            .AddHttpClient<IMawaqitApiService, MawaqitApiService>(config =>
            {
                config.Timeout = TimeSpan.FromSeconds(HTTP_REQUEST_TIMEOUT_SECONDS);
                config.BaseAddress = new Uri("https://mawaqit.net/de/");
            });
        AddResilienceAndThrottling(services, mawaqitClientBuilder, CreateThrottleOptions("Mawaqit", minRequestIntervalMs: 200));
    }

    private static readonly TimeSpan s_maxThrottleWait = TimeSpan.FromSeconds(RESILIENCE_ATTEMPT_TIMEOUT_SECONDS - 5);

    private static ApiThrottleOptions CreateThrottleOptions(string apiName, int minRequestIntervalMs)
    {
        return new ApiThrottleOptions(
            ApiName: apiName,
            MinRequestInterval: TimeSpan.FromMilliseconds(minRequestIntervalMs),
            MaxRequestInterval: TimeSpan.FromSeconds(2),
            DefaultCooldown: TimeSpan.FromSeconds(2),
            MaxCooldown: TimeSpan.FromMinutes(5),
            MaxWaitBeforeFailing: s_maxThrottleWait);
    }

    private static void AddResilientRefitClient<TClient>(IServiceCollection services, string baseAddress, ApiThrottleOptions throttleOptions)
        where TClient : class
    {
        IHttpClientBuilder httpClientBuilder = services
            .AddRefitGeneratedClient<TClient>(RefitJsonContext.Default)
            .ConfigureHttpClient(config =>
            {
                config.Timeout = TimeSpan.FromSeconds(HTTP_REQUEST_TIMEOUT_SECONDS);
                config.BaseAddress = new Uri(baseAddress);
            });

        AddResilienceAndThrottling(services, httpClientBuilder, throttleOptions);
    }

    /// <summary>
    /// Outer: the standard resilience pipeline (retries 429, 5xx, 408 and network errors with
    /// exponential backoff and honours Retry-After; timeouts; circuit breaker).
    /// Inner, i.e. for every single attempt: the adaptive throttle of this API.
    /// </summary>
    private static void AddResilienceAndThrottling(IServiceCollection services, IHttpClientBuilder httpClientBuilder, ApiThrottleOptions throttleOptions)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddKeyedSingleton(throttleOptions.ApiName, (sp, _) =>
            new ApiThrottle(throttleOptions, sp.GetRequiredService<TimeProvider>(), sp.GetRequiredService<ILogger<ApiThrottle>>()));

        httpClientBuilder.AddStandardResilienceHandler(options =>
        {
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(RESILIENCE_ATTEMPT_TIMEOUT_SECONDS);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(RESILIENCE_TOTAL_REQUEST_TIMEOUT_SECONDS);

            // validation requires it to be at least twice the attempt timeout
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(RESILIENCE_ATTEMPT_TIMEOUT_SECONDS * 2);
        });

        httpClientBuilder.AddHttpMessageHandler(sp =>
            new ApiThrottlingHandler(sp.GetRequiredKeyedService<ApiThrottle>(throttleOptions.ApiName)));
    }
}
