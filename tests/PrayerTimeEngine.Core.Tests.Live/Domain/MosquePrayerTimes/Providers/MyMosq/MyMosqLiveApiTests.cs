using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using PrayerTimeEngine.Core.Data.WebSocket;
using PrayerTimeEngine.Core.Data.WebSocket.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;
using PrayerTimeEngine.Core.Tests.Common;
using System.Net.WebSockets;

namespace PrayerTimeEngine.Core.Tests.Live.Domain.MosquePrayerTimes.Providers.MyMosq;

// These tests deliberately call the real MyMosq (Firebase WebSocket) API to check the
// fragile external integration. They are non-deterministic and require network access,
// so they live in their own project and run on a schedule (see the live-api-tests
// workflow) rather than on every push.
public class MyMosqLiveApiTests : BaseTest
{
    [Theory]
    [InlineData("1239")]
    [InlineData("1145")]
    [InlineData("1140")]
    public async Task GetPrayerTimesAsync_DifferentExternalIDs_NoErrors(string externalID)
    {
        // ARRANGE
        ServiceProvider serviceProvider = createServiceProvider(
            configureServiceCollection: serviceCollection =>
            {
                serviceCollection.AddSingleton(GetHandledDbContextFactory());
                serviceCollection.AddTransient<IMyMosqRepository, MyMosqRepository>();
                serviceCollection.AddTransient<IMyMosqApiService, MyMosqApiService>();
                serviceCollection.AddSingleton<IWebSocketClientFactory, WebSocketClientFactory>();
                serviceCollection.AddTransient<IWebSocketClient, WebSocketClient>(factory =>
                {
                    return new WebSocketClient(new ClientWebSocket());
                });
                serviceCollection.AddTransient<MyMosqMosquePrayerTimeProvider>();
            });

        // just some date but it has to be this year because the API returns data only for the current year
        var date = new LocalDate(DateTime.Today.Year, 1, 1);
        MyMosqMosquePrayerTimeProvider myMosqPrayerTimeService = serviceProvider.GetRequiredService<MyMosqMosquePrayerTimeProvider>();

        // ACT & ASSERT
        IMosqueDailyPrayerTimes result = await myMosqPrayerTimeService.GetPrayerTimesAsync(date, externalID, default);
    }

    [Theory]
    [InlineData("1239", true)]
    [InlineData("1145", true)]
    [InlineData("1140", true)]
    [InlineData("123497839", false)]
    public async Task ValidateData_DifferentExternalIDs_ValidatedAsExpected(
        string externalID, bool isValid)
    {
        // ARRANGE
        ServiceProvider serviceProvider = createServiceProvider(
            configureServiceCollection: serviceCollection =>
            {
                serviceCollection.AddSingleton(GetHandledDbContextFactory());
                serviceCollection.AddTransient<IMyMosqRepository, MyMosqRepository>();
                serviceCollection.AddTransient<IMyMosqApiService, MyMosqApiService>();
                serviceCollection.AddSingleton<IWebSocketClientFactory, WebSocketClientFactory>();
                serviceCollection.AddTransient<IWebSocketClient, WebSocketClient>(factory =>
                {
                    return new WebSocketClient(new ClientWebSocket());
                });
                serviceCollection.AddTransient<MyMosqMosquePrayerTimeProvider> ();
            });

        var date = new LocalDate(2024, 8, 30);
        MyMosqMosquePrayerTimeProvider myMosqPrayerTimeService = serviceProvider.GetRequiredService<MyMosqMosquePrayerTimeProvider>();

        // ACT
        bool result = await myMosqPrayerTimeService.ValidateData(externalID, default);

        // ASSERT
        result.Should().Be(isValid);
    }
}
