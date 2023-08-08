using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngineUnitTests.Mocks;
using System.Net;

namespace PrayerTimeEngineUnitTests.API.SemerkandAPI
{
    public class SemerkandPrayerTimeCalculatorTests
    {
        private SemerkandApiService getMockedSemerkandApiService()
        {
            Dictionary<string, string> urlToContentMap = new Dictionary<string, string>()
            {
                [$@"{SemerkandApiService.GET_COUNTRIES_URL}"] = File.ReadAllText(@"API\SemerkandAPI\TestData\Semerkand_TestCountriesData.txt"),
                [$@"{SemerkandApiService.GET_CITIES_BY_COUNTRY_URL}"] = File.ReadAllText(@"API\SemerkandAPI\TestData\Semerkand_TestCityData_Austria.txt"),
                [$@"{string.Format(SemerkandApiService.GET_TIMES_BY_CITY, "197", "2023")}"] = File.ReadAllText(@"API\SemerkandAPI\TestData\Semerkand_TestPrayerTimeData_20230729_Innsbruck.txt"),
            };

            var mockHttpMessageHandler = new MockHttpMessageHandler(HttpStatusCode.OK, urlToContentMap);
            var httpClient = new HttpClient(mockHttpMessageHandler);

            return new SemerkandApiService(httpClient);
        }

        [Test]
        public void SemerkandPrayerTimeCalculator_GetPrayerTimesAsyncWithNormalInput_PrayerTimesForThatDay()
        {
            // ARRANGE
            var semerkandDBAccess = new SemerkandDBAccess(new SQLiteDB());
            var semerkandApiService = getMockedSemerkandApiService();

            // Put together calculator
            var semerkandPrayerTimeCalculator =
                new SemerkandPrayerTimeCalculator(
                    semerkandDBAccess,
                    semerkandApiService, null);

            // ACT
            ICalculationPrayerTimes result =
                semerkandPrayerTimeCalculator.GetPrayerTimesAsync(
                    new DateTime(2023, 7, 29),
                    new List<GenericSettingConfiguration> { new GenericSettingConfiguration(ETimeType.DhuhrStart, calculationSource: ECalculationSource.Semerkand) }
                ).GetAwaiter().GetResult().Single().Key;

            SemerkandPrayerTimes semerkandPrayerTimes = result as SemerkandPrayerTimes;

            // ASSERT
            Assert.IsNotNull(semerkandPrayerTimes);

            Assert.That(semerkandPrayerTimes.Date, Is.EqualTo(new DateTime(2023, 7, 29)));
            Assert.That(semerkandPrayerTimes.Fajr, Is.EqualTo(new DateTime(2023, 7, 29, 03, 15, 0)));
            Assert.That(semerkandPrayerTimes.NextFajr, Is.EqualTo(new DateTime(2023, 7, 30, 03, 17, 0)));

            Assert.That(semerkandPrayerTimes.Shuruq, Is.EqualTo(new DateTime(2023, 7, 29, 05, 41, 0)));
            Assert.That(semerkandPrayerTimes.Dhuhr, Is.EqualTo(new DateTime(2023, 7, 29, 13, 26, 0)));
            Assert.That(semerkandPrayerTimes.Asr, Is.EqualTo(new DateTime(2023, 7, 29, 17, 30, 0)));
            Assert.That(semerkandPrayerTimes.Maghrib, Is.EqualTo(new DateTime(2023, 7, 29, 21, 00, 0)));
            Assert.That(semerkandPrayerTimes.Isha, Is.EqualTo(new DateTime(2023, 7, 29, 23, 02, 0)));
        }
    }
}