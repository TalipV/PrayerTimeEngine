using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlacesService.Interfaces;
using PrayerTimeEngine.Core.Tests.Common;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.Integration.FaziletAPI
{
    public class FaziletPrayerTimeCalculatorTests : BaseTest
    {
        protected override void ConfigureServiceProvider(ServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(Substitute.For<IPlaceService>());
            serviceCollection.AddSingleton<IFaziletDBAccess, FaziletDBAccess>();
            serviceCollection.AddSingleton<IFaziletApiService>(getMockedFaziletApiService());
            serviceCollection.AddSingleton(Substitute.For<ILogger<FaziletPrayerTimeCalculator>>());
            serviceCollection.AddSingleton<FaziletPrayerTimeCalculator>();
        }

        private static FaziletApiService getMockedFaziletApiService()
        {
            string dummyBaseURL = @"http://dummy.url.com";
            Dictionary<string, string> urlToContentMap = new Dictionary<string, string>()
            {
                [$@"{dummyBaseURL}/{FaziletApiService.GET_COUNTRIES_URL}"] = File.ReadAllText(@$"{TEST_DATA_FILE_PATH}\FaziletTestData\Fazilet_TestCountriesData.txt"),
                [$@"{dummyBaseURL}/{FaziletApiService.GET_CITIES_BY_COUNTRY_URL}2"] = File.ReadAllText(@$"{TEST_DATA_FILE_PATH}\FaziletTestData\Fazilet_TestCityData_Austria.txt"),
                [$@"{dummyBaseURL}/{string.Format(FaziletApiService.GET_TIMES_BY_CITY_URL, "92")}"] = File.ReadAllText(@$"{TEST_DATA_FILE_PATH}\FaziletTestData\Fazilet_TestPrayerTimeData_20230729_Innsbruck.txt"),
            };

            var mockHttpMessageHandler = new MockHttpMessageHandler(HttpStatusCode.OK, urlToContentMap);
            var httpClient = new HttpClient(mockHttpMessageHandler)
            {
                BaseAddress = new Uri(dummyBaseURL)
            };

            return new FaziletApiService(httpClient);
        }

        [Test]
        public async Task FaziletPrayerTimeCalculator_GetPrayerTimesAsyncWithNormalInput_PrayerTimesForThatDay()
        {
            // ARRANGE
            FaziletPrayerTimeCalculator faziletPrayerTimeCalculator = ServiceProvider.GetService<FaziletPrayerTimeCalculator>();

            // ACT
            ICalculationPrayerTimes result =
                (await faziletPrayerTimeCalculator.GetPrayerTimesAsync(
                    new LocalDate(2023, 7, 29),
                    new FaziletLocationData { CountryName = "Avusturya", CityName = "Innsbruck" },
                    [new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = ECalculationSource.Fazilet }]
                ).ConfigureAwait(false)).Single().Key;

            FaziletPrayerTimes faziletPrayerTimes = result as FaziletPrayerTimes;

            // ASSERT
            faziletPrayerTimes.Should().NotBeNull();

            faziletPrayerTimes.Date.Should().Be(new LocalDate(2023, 7, 29));
            faziletPrayerTimes.Imsak.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 03, 04, 0));
            faziletPrayerTimes.Fajr.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 03, 24, 0));
            faziletPrayerTimes.NextFajr.Value.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 03, 27, 0));

            faziletPrayerTimes.Shuruq.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 05, 43, 0));
            faziletPrayerTimes.Dhuhr.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 13, 28, 0));
            faziletPrayerTimes.Asr.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 17, 31, 0));
            faziletPrayerTimes.Maghrib.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 21, 02, 0));
            faziletPrayerTimes.Isha.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 23, 11, 0));
        }
    }
}