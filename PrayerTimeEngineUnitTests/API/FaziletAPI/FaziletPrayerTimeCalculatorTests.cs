using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngineUnitTests.Mocks;
using System.Net;

namespace PrayerTimeEngineUnitTests.API.FaziletAPI
{
    public class FaziletPrayerTimeCalculatorTests
    {
        private FaziletApiService getMockedFaziletApiService()
        {
            string dummyBaseURL = @"http://dummy.url.com";
            Dictionary<string, string> urlToContentMap = new Dictionary<string, string>()
            {
                [$@"{dummyBaseURL}/{FaziletApiService.GET_COUNTRIES_URL}"] = File.ReadAllText(@"API\FaziletAPI\TestData\Fazilet_TestCountriesData.txt"),
                [$@"{dummyBaseURL}/{FaziletApiService.GET_CITIES_BY_COUNTRY_URL}2"] = File.ReadAllText(@"API\FaziletAPI\TestData\Fazilet_TestCityData_Austria.txt"),
                [$@"{dummyBaseURL}/{string.Format(FaziletApiService.GET_TIMES_BY_CITY_URL, "92")}"] = File.ReadAllText(@"API\FaziletAPI\TestData\Fazilet_TestPrayerTimeData_20230729_Innsbruck.txt"),
            };

            var mockHttpMessageHandler = new MockHttpMessageHandler(HttpStatusCode.OK, urlToContentMap);
            var httpClient = new HttpClient(mockHttpMessageHandler)
            {
                BaseAddress = new Uri(dummyBaseURL)
            };

            return new FaziletApiService(httpClient);
        }

        [Test]
        public void FaziletPrayerTimeCalculator_GetPrayerTimesAsyncWithNormalInput_PrayerTimesForThatDay()
        {
            // ARRANGE
            var faziletDBAccess = new FaziletDBAccess(new SQLiteDB());
            var faziletApiService = getMockedFaziletApiService();

            // Put together calculator
            var faziletPrayerTimeCalculator =
                new FaziletPrayerTimeCalculator(
                    faziletDBAccess,
                    faziletApiService,
                    null,
                    null);

            // ACT
            ICalculationPrayerTimes result =
                faziletPrayerTimeCalculator.GetPrayerTimesAsync(
                    new DateTime(2023, 7, 29),
                    new FaziletLocationData { CountryName = "Avusturya", CityName = "Innsbruck" },
                    new List<GenericSettingConfiguration> { new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = ECalculationSource.Fazilet }}
                ).GetAwaiter().GetResult().Single().Key;

            FaziletPrayerTimes faziletPrayerTimes = result as FaziletPrayerTimes;

            // ASSERT
            Assert.IsNotNull(faziletPrayerTimes);

            Assert.That(faziletPrayerTimes.Date, Is.EqualTo(new DateTime(2023, 7, 29)));
            Assert.That(faziletPrayerTimes.Imsak, Is.EqualTo(new DateTime(2023, 7, 29, 03, 04, 0)));
            Assert.That(faziletPrayerTimes.Fajr, Is.EqualTo(new DateTime(2023, 7, 29, 03, 24, 0)));
            Assert.That(faziletPrayerTimes.NextFajr, Is.EqualTo(new DateTime(2023, 7, 30, 03, 27, 0)));

            Assert.That(faziletPrayerTimes.Shuruq, Is.EqualTo(new DateTime(2023, 7, 29, 05, 43, 0)));
            Assert.That(faziletPrayerTimes.Dhuhr, Is.EqualTo(new DateTime(2023, 7, 29, 13, 28, 0)));
            Assert.That(faziletPrayerTimes.Asr, Is.EqualTo(new DateTime(2023, 7, 29, 17, 31, 0)));
            Assert.That(faziletPrayerTimes.Maghrib, Is.EqualTo(new DateTime(2023, 7, 29, 21, 02, 0)));
            Assert.That(faziletPrayerTimes.Isha, Is.EqualTo(new DateTime(2023, 7, 29, 23, 11, 0)));
        }
    }
}