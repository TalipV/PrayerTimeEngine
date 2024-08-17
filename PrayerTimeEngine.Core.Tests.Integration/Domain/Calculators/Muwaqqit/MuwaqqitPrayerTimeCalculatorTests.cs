using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Integration.Domain.Calculators.Muwaqqit
{
    public class MuwaqqitPrayerTimeCalculatorTests : BaseTest
    {
        [Fact]
        public async Task GetPrayerTimesAsync_NormalInput_PrayerTimesForThatDay()
        {
            // ARRANGE
            ServiceProvider serviceProvider = createServiceProvider(
                configureServiceCollection: serviceCollection =>
                {
                    serviceCollection.AddSingleton(GetHandledDbContext());
                    serviceCollection.AddSingleton(Substitute.For<IPlaceService>());
                    serviceCollection.AddSingleton<TimeTypeAttributeService>();

                    serviceCollection.AddSingleton(Substitute.For<ILogger<MuwaqqitDBAccess>>());
                    serviceCollection.AddSingleton<IMuwaqqitDBAccess, MuwaqqitDBAccess>();
                    serviceCollection.AddSingleton<IMuwaqqitApiService>(SubstitutionHelper.GetMockedMuwaqqitApiService());
                    serviceCollection.AddSingleton(Substitute.For<ILogger<MuwaqqitPrayerTimeCalculator>>());
                    serviceCollection.AddSingleton<MuwaqqitPrayerTimeCalculator>();
                });

            var date = new LocalDate(2023, 7, 30);
            var locationData = new MuwaqqitLocationData
            {
                Latitude = 47.2803835M,
                Longitude = 11.41337M,
                TimezoneName = TestDataHelper.EUROPE_VIENNA_TIME_ZONE.Id
            };
            var dateTimeZone = DateTimeZoneProviders.Tzdb[locationData.TimezoneName];
            List<GenericSettingConfiguration> configs =
                [
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrStart, Degree = -12.0 },
                    new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, Source = ECalculationSource.Muwaqqit },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Degree = -7.5 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Degree = -4.5 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.DuhaStart, Degree = 3.5 },
                    new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = ECalculationSource.Muwaqqit },
                    new GenericSettingConfiguration { TimeType = ETimeType.DhuhrEnd, Source = ECalculationSource.Muwaqqit },
                    new GenericSettingConfiguration { TimeType = ETimeType.AsrStart, Source = ECalculationSource.Muwaqqit },
                    new GenericSettingConfiguration { TimeType = ETimeType.AsrEnd, Source = ECalculationSource.Muwaqqit },
                    new GenericSettingConfiguration { TimeType = ETimeType.AsrMithlayn, Source = ECalculationSource.Muwaqqit },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.AsrKaraha, Degree = 4.5 },
                    new GenericSettingConfiguration { TimeType = ETimeType.MaghribStart, Source = ECalculationSource.Muwaqqit },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribEnd, Degree = -12.0 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribIshtibaq, Degree = -8 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.IshaStart, Degree = -15.5 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.IshaEnd, Degree = -15.0 },
                ];

            MuwaqqitPrayerTimeCalculator muwaqqitPrayerTimeCalculator = serviceProvider.GetService<MuwaqqitPrayerTimeCalculator>();

            // ACT
            List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
                await muwaqqitPrayerTimeCalculator.GetPrayerTimesAsync(
                    date.AtStartOfDayInZone(dateTimeZone),
                    locationData,
                    configs, 
                    default);

            // ASSERT
            result.FirstOrDefault(x => x.TimeType == ETimeType.FajrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 04, 27, 04));
            result.FirstOrDefault(x => x.TimeType == ETimeType.FajrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 05, 49, 53));
            result.FirstOrDefault(x => x.TimeType == ETimeType.FajrGhalas).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 05, 02, 27));
            result.FirstOrDefault(x => x.TimeType == ETimeType.FajrKaraha).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 05, 20, 25));
            
            result.FirstOrDefault(x => x.TimeType == ETimeType.DuhaStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 06, 17, 04));
           
            result.FirstOrDefault(x => x.TimeType == ETimeType.DhuhrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 13, 21, 22));
            result.FirstOrDefault(x => x.TimeType == ETimeType.DhuhrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 17, 25, 53));
            
            result.FirstOrDefault(x => x.TimeType == ETimeType.AsrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 17, 25, 53));
            result.FirstOrDefault(x => x.TimeType == ETimeType.AsrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 20, 50, 59));
            result.FirstOrDefault(x => x.TimeType == ETimeType.AsrMithlayn).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 18, 33, 27));
            result.FirstOrDefault(x => x.TimeType == ETimeType.AsrKaraha).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 20, 17, 15));
            
            result.FirstOrDefault(x => x.TimeType == ETimeType.MaghribStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 20, 50, 59));
            result.FirstOrDefault(x => x.TimeType == ETimeType.MaghribEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 22, 13, 17));
            result.FirstOrDefault(x => x.TimeType == ETimeType.MaghribIshtibaq).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 21, 41, 46));
            
            result.FirstOrDefault(x => x.TimeType == ETimeType.IshaStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 22, 44, 14));
            result.FirstOrDefault(x => x.TimeType == ETimeType.IshaStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 22, 44, 14));
            result.FirstOrDefault(x => x.TimeType == ETimeType.IshaEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 31, 04, 02, 30));
        }
    }
}