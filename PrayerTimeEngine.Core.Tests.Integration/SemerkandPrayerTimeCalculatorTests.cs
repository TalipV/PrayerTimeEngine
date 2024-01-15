using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlacesService.Interfaces;
using PrayerTimeEngine.Core.Tests.Common;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.Integration
{
    public class SemerkandPrayerTimeCalculatorTests : BaseTest
    {
        protected override void ConfigureServiceProvider(ServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(Substitute.For<IPlaceService>());

            serviceCollection.AddSingleton<ISemerkandDBAccess, SemerkandDBAccess>();
            serviceCollection.AddSingleton<ISemerkandApiService>(getMockedSemerkandApiService());
            serviceCollection.AddSingleton(Substitute.For<ILogger<SemerkandPrayerTimeCalculator>>());
            serviceCollection.AddSingleton<SemerkandPrayerTimeCalculator>();
        }

        private static SemerkandApiService getMockedSemerkandApiService()
        {
            Dictionary<string, string> urlToContentMap = new Dictionary<string, string>()
            {
                [$@"{SemerkandApiService.GET_COUNTRIES_URL}"] = File.ReadAllText(@$"{TEST_DATA_FILE_PATH}\SemerkandTestData\Semerkand_TestCountriesData.txt"),
                [$@"{SemerkandApiService.GET_CITIES_BY_COUNTRY_URL}"] = File.ReadAllText(@$"{TEST_DATA_FILE_PATH}\SemerkandTestData\Semerkand_TestCityData_Austria.txt"),
                [$@"{string.Format(SemerkandApiService.GET_TIMES_BY_CITY, "197", "2023")}"] = File.ReadAllText(@$"{TEST_DATA_FILE_PATH}\SemerkandTestData\Semerkand_TestPrayerTimeData_20230729_Innsbruck.txt"),
            };

            var mockHttpMessageHandler = new MockHttpMessageHandler(HttpStatusCode.OK, urlToContentMap);
            var httpClient = new HttpClient(mockHttpMessageHandler);

            return new SemerkandApiService(httpClient);
        }

        [Test]
        public async Task SemerkandPrayerTimeCalculator_GetPrayerTimesAsyncWithNormalInput_PrayerTimesForThatDay()
        {
            // ARRANGE
            SemerkandPrayerTimeCalculator semerkandPrayerTimeCalculator = ServiceProvider.GetService<SemerkandPrayerTimeCalculator>();

            // ACT
            ICalculationPrayerTimes result =
                (await semerkandPrayerTimeCalculator.GetPrayerTimesAsync(
                    new LocalDate(2023, 7, 29),
                    new SemerkandLocationData
                    {
                        CountryName = "Avusturya",
                        CityName = "Innsbruck",
                        TimezoneName = "Europe/Vienna"
                    },
                    [new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = ECalculationSource.Semerkand }]
                ).ConfigureAwait(false)).Single().Key;

            SemerkandPrayerTimes semerkandPrayerTimes = result as SemerkandPrayerTimes;

            // ASSERT
            semerkandPrayerTimes.Should().NotBeNull();

            semerkandPrayerTimes.Date.Should().Be(new LocalDate(2023, 7, 29));
            semerkandPrayerTimes.Fajr.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 03, 15, 0));
            semerkandPrayerTimes.NextFajr.Value.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 03, 17, 0));

            semerkandPrayerTimes.Shuruq.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 05, 41, 0));
            semerkandPrayerTimes.Dhuhr.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 13, 26, 0));
            semerkandPrayerTimes.Asr.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 17, 30, 0));
            semerkandPrayerTimes.Maghrib.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 21, 00, 0));
            semerkandPrayerTimes.Isha.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 23, 02, 0));
        }
    }
}