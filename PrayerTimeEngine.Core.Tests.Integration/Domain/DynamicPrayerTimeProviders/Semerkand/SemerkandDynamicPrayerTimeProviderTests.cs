using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Integration.Domain.DynamicPrayerTimeProviders.Semerkand
{
    public class SemerkandDynamicPrayerTimeProviderTests : BaseTest
    {
        [Fact]
        public async Task GetPrayerTimesAsync_NormalInput_PrayerTimesForThatDay()
        {
            // ARRANGE
            ServiceProvider serviceProvider = createServiceProvider(
                configureServiceCollection: serviceCollection =>
                {
                    serviceCollection.AddSingleton(GetHandledDbContextFactory());
                    serviceCollection.AddSingleton(Substitute.For<IPlaceService>());

                    serviceCollection.AddSingleton<ISemerkandDBAccess, SemerkandDBAccess>();
                    serviceCollection.AddSingleton(SubstitutionHelper.GetMockedSemerkandApiService());
                    serviceCollection.AddSingleton(Substitute.For<ILogger<SemerkandDynamicPrayerTimeProvider>>());
                    serviceCollection.AddSingleton<SemerkandDynamicPrayerTimeProvider>();
                });

            SemerkandDynamicPrayerTimeProvider semerkandDynamicPrayerTimeProvider = serviceProvider.GetRequiredService<SemerkandDynamicPrayerTimeProvider>();

            var date = new LocalDate(2023, 7, 29);
            var locationData = new SemerkandLocationData
            {
                CountryName = "Avusturya",
                CityName = "Innsbruck",
                TimezoneName = TestDataHelper.EUROPE_VIENNA_TIME_ZONE.Id
            };
            var dateTimeZone = DateTimeZoneProviders.Tzdb[locationData.TimezoneName];

            List<GenericSettingConfiguration> configs =
                [
                    new GenericSettingConfiguration { TimeType = ETimeType.FajrStart, Source = EDynamicPrayerTimeProviderType.Semerkand },
                    new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, Source = EDynamicPrayerTimeProviderType.Semerkand },
                    new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = EDynamicPrayerTimeProviderType.Semerkand },
                    new GenericSettingConfiguration { TimeType = ETimeType.DhuhrEnd, Source = EDynamicPrayerTimeProviderType.Semerkand },
                    new GenericSettingConfiguration { TimeType = ETimeType.AsrStart, Source = EDynamicPrayerTimeProviderType.Semerkand },
                    new GenericSettingConfiguration { TimeType = ETimeType.AsrEnd, Source = EDynamicPrayerTimeProviderType.Semerkand },
                    new GenericSettingConfiguration { TimeType = ETimeType.MaghribStart, Source = EDynamicPrayerTimeProviderType.Semerkand },
                    new GenericSettingConfiguration { TimeType = ETimeType.MaghribEnd, Source = EDynamicPrayerTimeProviderType.Semerkand },
                    new GenericSettingConfiguration { TimeType = ETimeType.IshaStart, Source = EDynamicPrayerTimeProviderType.Semerkand },
                    new GenericSettingConfiguration { TimeType = ETimeType.IshaEnd, Source = EDynamicPrayerTimeProviderType.Semerkand },
                ];

            // ACT
            List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
                await semerkandDynamicPrayerTimeProvider.GetPrayerTimesAsync(
                    date.AtStartOfDayInZone(dateTimeZone),
                    locationData,
                    configs,
                    default);

            // ASSERT
            result.Should().NotBeNull();

            result.FirstOrDefault(x => x.TimeType == ETimeType.FajrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 03, 15, 0));
            result.FirstOrDefault(x => x.TimeType == ETimeType.FajrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 05, 41, 0));

            result.FirstOrDefault(x => x.TimeType == ETimeType.DhuhrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 13, 26, 0));
            result.FirstOrDefault(x => x.TimeType == ETimeType.DhuhrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 17, 30, 0));

            result.FirstOrDefault(x => x.TimeType == ETimeType.AsrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 17, 30, 0));
            result.FirstOrDefault(x => x.TimeType == ETimeType.AsrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 21, 00, 0));

            result.FirstOrDefault(x => x.TimeType == ETimeType.MaghribStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 21, 00, 0));
            result.FirstOrDefault(x => x.TimeType == ETimeType.MaghribEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 23, 02, 0));

            result.FirstOrDefault(x => x.TimeType == ETimeType.IshaStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 23, 02, 0));
            result.FirstOrDefault(x => x.TimeType == ETimeType.IshaEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 03, 17, 0));
        }
    }
}