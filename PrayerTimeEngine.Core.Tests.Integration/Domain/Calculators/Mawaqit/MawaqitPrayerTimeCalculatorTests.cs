using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Integration.Domain.Calculators.Mawaqit
{
    public class MawaqitDynamicPrayerTimeProviderTests : BaseTest
    {
        [Fact]
        public async Task GetPrayerTimesAsync_NormalInput_PrayerTimesForThatDay()
        {
            // ARRANGE
            ServiceProvider serviceProvider = createServiceProvider(
                configureServiceCollection: serviceCollection =>
                {
                    serviceCollection.AddSingleton(GetHandledDbContextFactory());
                    serviceCollection.AddSingleton<IMawaqitDBAccess, MawaqitDBAccess>();
                    serviceCollection.AddSingleton(SubstitutionHelper.GetMockedMawaqitApiService());
                    serviceCollection.AddSingleton<MawaqitPrayerTimeService>();
                });
            var date = new LocalDate(2024, 8, 29);
            string externalID = "hamza-koln";
            MawaqitPrayerTimeService mawaqitPrayerTimeService = serviceProvider.GetRequiredService<MawaqitPrayerTimeService>();

            // ACT
            IMosquePrayerTimes result = await mawaqitPrayerTimeService.GetPrayerTimesAsync(date, externalID, default);

            // ASSERT
            result.Should().NotBeNull();

            result.Date.Should().Be(new LocalDate(2024, 8, 29));
            result.ExternalID.Should().Be(externalID);

            result.Fajr.Should().Be(new LocalTime(05, 05, 00));
            result.FajrCongregation.Should().Be(new LocalTime(05, 35, 00));
            result.Shuruq.Should().Be(new LocalTime(06, 35, 00));
            result.Dhuhr.Should().Be(new LocalTime(13, 35, 00));
            result.DhuhrCongregation.Should().Be(new LocalTime(13, 45, 00));
            result.Asr.Should().Be(new LocalTime(17, 22, 00));
            result.AsrCongregation.Should().Be(new LocalTime(17, 32, 00));
            result.Maghrib.Should().Be(new LocalTime(20, 30, 00));
            result.MaghribCongregation.Should().Be(new LocalTime(20, 35, 00));
            result.Isha.Should().Be(new LocalTime(22, 06, 00));
            result.IshaCongregation.Should().Be(new LocalTime(22, 16, 00));

            result.Jumuah.Should().Be(new LocalTime(14, 30, 00));
            result.Jumuah2.Should().BeNull();
        }
    }
}