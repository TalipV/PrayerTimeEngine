using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Integration.Domain.Calculators.Fazilet
{
    public class FaziletPrayerTimeCalculatorTests : BaseTest
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
                    serviceCollection.AddSingleton<IFaziletDBAccess, FaziletDBAccess>();
                    serviceCollection.AddSingleton<IFaziletApiService>(SubstitutionHelper.GetMockedFaziletApiService());
                    serviceCollection.AddSingleton(Substitute.For<ILogger<FaziletPrayerTimeCalculator>>());
                    serviceCollection.AddSingleton<FaziletPrayerTimeCalculator>();
                });
            FaziletPrayerTimeCalculator faziletPrayerTimeCalculator = serviceProvider.GetService<FaziletPrayerTimeCalculator>();

            List<GenericSettingConfiguration> configs =
                [
                    new GenericSettingConfiguration { TimeType = ETimeType.FajrStart, Source = ECalculationSource.Fazilet },
                    new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, Source = ECalculationSource.Fazilet },
                    new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = ECalculationSource.Fazilet },
                    new GenericSettingConfiguration { TimeType = ETimeType.DhuhrEnd, Source = ECalculationSource.Fazilet },
                    new GenericSettingConfiguration { TimeType = ETimeType.AsrStart, Source = ECalculationSource.Fazilet },
                    new GenericSettingConfiguration { TimeType = ETimeType.AsrEnd, Source = ECalculationSource.Fazilet },
                    new GenericSettingConfiguration { TimeType = ETimeType.MaghribStart, Source = ECalculationSource.Fazilet },
                    new GenericSettingConfiguration { TimeType = ETimeType.MaghribEnd, Source = ECalculationSource.Fazilet },
                    new GenericSettingConfiguration { TimeType = ETimeType.IshaStart, Source = ECalculationSource.Fazilet },
                    new GenericSettingConfiguration { TimeType = ETimeType.IshaEnd, Source = ECalculationSource.Fazilet },
                ];

            // ACT
            List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
                await faziletPrayerTimeCalculator.GetPrayerTimesAsync(
                    new LocalDate(2023, 7, 29).AtStartOfDayInZone(TestDataHelper.EUROPE_VIENNA_TIME_ZONE),
                    new FaziletLocationData { CountryName = "Avusturya", CityName = "Innsbruck" },
                    configs, 
                    default);

            // ASSERT
            result.Should().NotBeNull();

            result.FirstOrDefault(x => x.TimeType == ETimeType.FajrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 03, 24, 0));
            result.FirstOrDefault(x => x.TimeType == ETimeType.FajrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 05, 43, 0));
            
            result.FirstOrDefault(x => x.TimeType == ETimeType.DhuhrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 13, 28, 0));
            result.FirstOrDefault(x => x.TimeType == ETimeType.DhuhrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 17, 31, 0));
            
            result.FirstOrDefault(x => x.TimeType == ETimeType.AsrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 17, 31, 0));
            result.FirstOrDefault(x => x.TimeType == ETimeType.AsrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 21, 02, 0));
            
            result.FirstOrDefault(x => x.TimeType == ETimeType.MaghribStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 21, 02, 0));
            result.FirstOrDefault(x => x.TimeType == ETimeType.MaghribEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 23, 11, 0));
            
            result.FirstOrDefault(x => x.TimeType == ETimeType.IshaStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 23, 11, 0));
            result.FirstOrDefault(x => x.TimeType == ETimeType.IshaEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 03, 27, 0));
        }
    }
}