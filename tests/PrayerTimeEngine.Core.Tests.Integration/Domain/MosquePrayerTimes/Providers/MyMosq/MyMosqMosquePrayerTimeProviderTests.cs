using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

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
                serviceCollection.AddSingleton<IMyMosqRepository, MyMosqRepository>();
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

        result.Date.Should().Be(new LocalDate(2024, 8, 30)); // friday
        result.ExternalID.Should().Be(externalID);

        result.Fajr.Should().Be(new LocalTime(04, 38, 00));
        result.FajrCongregation.Should().Be(new LocalTime(05, 51, 00));
        result.Shuruq.Should().Be(new LocalTime(06, 36, 00));
        result.Dhuhr.Should().Be(new LocalTime(13, 38, 00));
        result.DhuhrCongregation.Should().Be(new LocalTime(14, 30, 00));
        result.Asr.Should().Be(new LocalTime(17, 22, 00));
        result.AsrCongregation.Should().Be(new LocalTime(17, 22, 00));
        result.Maghrib.Should().Be(new LocalTime(20, 30, 00));
        result.MaghribCongregation.Should().Be(new LocalTime(20, 30, 00));
        result.Isha.Should().Be(new LocalTime(22, 00, 00));
        result.IshaCongregation.Should().Be(new LocalTime(22, 00, 00));

        result.Jumuah.Should().Be(new LocalTime(14, 30, 0));
        result.Jumuah2.Should().BeNull();
    }
}