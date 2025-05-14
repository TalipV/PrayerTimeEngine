using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using PrayerTimeEngine.Core.Data.WebSocket;
using PrayerTimeEngine.Core.Data.WebSocket.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using System.Net.WebSockets;

namespace PrayerTimeEngine.Core.Tests.Integration.Domain.MosquePrayerTimes.Providers.MyMosq;


public class MyMosqMosquePrayerTimeProviderTests : BaseTest
{
    [Fact]
    public async Task GetPrayerTimesAsync_NormalInput_PrayerTimesForThatDay()
    {
        // ARRANGE
        ServiceProvider serviceProvider = createServiceProvider(
            configureServiceCollection: serviceCollection =>
            {
                serviceCollection.AddSingleton(GetHandledDbContextFactory());
                serviceCollection.AddSingleton<IMyMosqDBAccess, MyMosqDBAccess>();
                serviceCollection.AddSingleton<IMyMosqApiService, MyMosqApiService>(factory => SubstitutionHelper.GetMockedMyMosqApiService());
                serviceCollection.AddSingleton<MyMosqMosquePrayerTimeProvider>();
            });
        var date = new LocalDate(2024, 8, 30);
        string externalID = "1239";

        MyMosqMosquePrayerTimeProvider myMosqMosquePrayerTimeProvider = serviceProvider.GetRequiredService<MyMosqMosquePrayerTimeProvider>();

        // ACT
        IMosqueDailyPrayerTimes result = await myMosqMosquePrayerTimeProvider.GetPrayerTimesAsync(date, externalID, default);

        // ASSERT
        result.Should().NotBeNull();

        result.Date.Should().Be(new LocalDate(2024, 8, 30));
        result.ExternalID.Should().Be(externalID);

        result.Fajr.Should().Be(new LocalTime(04, 38, 00));
        result.FajrCongregation.Should().Be(new LocalTime(05, 51, 00));
        result.Shuruq.Should().Be(new LocalTime(06, 36, 00));
        result.Dhuhr.Should().Be(new LocalTime(14, 30, 00));    // WHY? BECAUSE OF JUMU'AH? THEN WHY IS THE JUMU'AH FIELD 0:00?
        result.DhuhrCongregation.Should().Be(new LocalTime(13, 38, 00));
        result.Asr.Should().Be(new LocalTime(17, 22, 00));
        result.AsrCongregation.Should().Be(new LocalTime(17, 22, 00));
        result.Maghrib.Should().Be(new LocalTime(20, 30, 00));
        result.MaghribCongregation.Should().Be(new LocalTime(20, 30, 00));
        result.Isha.Should().Be(new LocalTime(22, 00, 00));
        result.IshaCongregation.Should().Be(new LocalTime(22, 00, 00));

        result.Jumuah.Should().Be(new LocalTime(14, 30, 0));
        result.Jumuah2.Should().BeNull();
    }

    // to check the fragile API implementation with a live API call because why not
    [Theory]
    [InlineData("1239")]
    [InlineData("1145")]
    [InlineData("1140")]
    public async Task GetPrayerTimesAsync_DifferentExternalIDsWithLiveApi_NoErrors(string externalID)
    {
        // ARRANGE
        ServiceProvider serviceProvider = createServiceProvider(
            configureServiceCollection: serviceCollection =>
            {
                serviceCollection.AddSingleton(GetHandledDbContextFactory());
                serviceCollection.AddTransient<IMyMosqDBAccess, MyMosqDBAccess>();
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

    // to check the fragile API implementation with a live API call because why not
    [Theory]
    [InlineData("1239", true)]
    [InlineData("1145", true)]
    [InlineData("1140", true)]
    [InlineData("123497839", false)]
    public async Task ValidateData_DifferentExternalIDsWithLiveApi_ValidatedAsExpected(
        string externalID, bool isValid)
    {
        // ARRANGE
        ServiceProvider serviceProvider = createServiceProvider(
            configureServiceCollection: serviceCollection =>
            {
                serviceCollection.AddSingleton(GetHandledDbContextFactory());
                serviceCollection.AddTransient<IMyMosqDBAccess, MyMosqDBAccess>();
                serviceCollection.AddTransient<IMyMosqApiService, MyMosqApiService>();
                serviceCollection.AddSingleton<IWebSocketClientFactory, WebSocketClientFactory>();
                serviceCollection.AddTransient<IWebSocketClient, WebSocketClient>(factory =>
                {
                    return new WebSocketClient(new ClientWebSocket());
                });
                serviceCollection.AddTransient<MyMosqMosquePrayerTimeProvider>();
            });

        var date = new LocalDate(2024, 8, 30);
        MyMosqMosquePrayerTimeProvider myMosqPrayerTimeService = serviceProvider.GetRequiredService<MyMosqMosquePrayerTimeProvider>();

        // ACT
        bool result = await myMosqPrayerTimeService.ValidateData(externalID, default);

        // ASSERT
        result.Should().Be(isValid);
    }
}