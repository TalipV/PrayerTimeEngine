using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Common.Extension;
using PrayerTimeEngine.Domain;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngineUnitTests.Mocks;
using System.Net;

namespace PrayerTimeEngineUnitTests.API.MuwaqqitAPI
{
    public class MuwaqqitPrayerTimeCalculatorTests
    {
        private MuwaqqitApiService getMockedMuwaqqitApiService()
        {
            Dictionary<string, string> urlToContentMap = new Dictionary<string, string>()
            {
                [@"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2fVienna&fa=-12&ea=-12&isn=-8&ia=3.5"] = File.ReadAllText(@"API\MuwaqqitAPI\TestData\Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Part1.txt"),
                [@"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2fVienna&fa=-7.5&ea=-15.5&isn=-12&ia=4.5"] = File.ReadAllText(@"API\MuwaqqitAPI\TestData\Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Part2.txt"),
                [@"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2fVienna&fa=-4.5&ea=-12&isn=-12&ia=-12"] = File.ReadAllText(@"API\MuwaqqitAPI\TestData\Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Part3.txt"),
                [@"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2fVienna&fa=-15&ea=-12&isn=-12&ia=-12"] = File.ReadAllText(@"API\MuwaqqitAPI\TestData\Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Part3.txt"),
            };

            var mockHttpMessageHandler = new MockHttpMessageHandler(HttpStatusCode.OK, urlToContentMap);
            var httpClient = new HttpClient(mockHttpMessageHandler);

            return new MuwaqqitApiService(httpClient);
        }

        [Test]
        public void MuwaqqitPrayerTimeCalculator_GetPrayerTimesAsyncWithNormalInput_PrayerTimesForThatDay()
        {
            // ARRANGE
            DateTime testDate = new DateTime(2023, 7, 30);
            var configs =
                new List<GenericSettingConfiguration>
                {
                    new MuwaqqitDegreeCalculationConfiguration(ETimeType.FajrStart, 0, -12.0),
                    new GenericSettingConfiguration(ETimeType.FajrEnd, 0, ECalculationSource.Muwaqqit),
                    new MuwaqqitDegreeCalculationConfiguration(ETimeType.FajrGhalas, 0, -7.5),
                    new MuwaqqitDegreeCalculationConfiguration(ETimeType.FajrKaraha, 0, -4.5),

                    new MuwaqqitDegreeCalculationConfiguration(ETimeType.DuhaStart, 0, 3.5),

                    new GenericSettingConfiguration(ETimeType.DhuhrStart, 0, ECalculationSource.Muwaqqit),
                    new GenericSettingConfiguration(ETimeType.DhuhrEnd, 0, ECalculationSource.Muwaqqit),

                    new GenericSettingConfiguration(ETimeType.AsrStart, 0, ECalculationSource.Muwaqqit),
                    new GenericSettingConfiguration(ETimeType.AsrEnd, 0, ECalculationSource.Muwaqqit),
                    new GenericSettingConfiguration(ETimeType.AsrMithlayn, 0, ECalculationSource.Muwaqqit),
                    new MuwaqqitDegreeCalculationConfiguration(ETimeType.AsrKaraha, 0, 4.5),

                    new GenericSettingConfiguration(ETimeType.MaghribStart, 0, ECalculationSource.Muwaqqit),
                    new MuwaqqitDegreeCalculationConfiguration(ETimeType.MaghribEnd, 0, -12.0),
                    new MuwaqqitDegreeCalculationConfiguration(ETimeType.MaghribIshtibaq, 0, -8),

                    new MuwaqqitDegreeCalculationConfiguration(ETimeType.IshaStart, 0, -15.5),
                    new MuwaqqitDegreeCalculationConfiguration(ETimeType.IshaEnd, 0, -15.0),
                };

            var muwaqqitDBAccess = new MuwaqqitDBAccess(new SQLiteDB());
            var muwaqqitApiService = getMockedMuwaqqitApiService();

            // Put together calculator
            var muwaqqitPrayerTimeCalculator =
                new MuwaqqitPrayerTimeCalculator(
                    muwaqqitDBAccess,
                    muwaqqitApiService,
                    new TimeTypeAttributeService());

            // ACT
            ILookup<ICalculationPrayerTimes, ETimeType> result =
                muwaqqitPrayerTimeCalculator.GetPrayerTimesAsync(
                    testDate,
                    configs
                ).GetAwaiter().GetResult();

            IDictionary<ETimeType, MuwaqqitPrayerTimes> timeTypeByCalculationPrayerTimes =
                result
                .SelectMany(pair => pair.Select(value => new { Key = value, Value = pair.Key }))
                .ToDictionary(x => x.Key, x => x.Value as MuwaqqitPrayerTimes);

            // ASSERT
            Assert.That(timeTypeByCalculationPrayerTimes.Count, Is.EqualTo(16));

            foreach (ICalculationPrayerTimes item in result.Select(x => x.Key))
            {
                Assert.That(item.Date, Is.EqualTo(new DateTime(2023, 7, 30)));
            }

            Assert.That(getMappedValue(ETimeType.FajrStart, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 04, 27, 04)));
            Assert.That(getMappedValue(ETimeType.FajrEnd, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 05, 49, 53)));
            Assert.That(getMappedValue(ETimeType.FajrGhalas, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 05, 02, 27)));
            Assert.That(getMappedValue(ETimeType.FajrKaraha, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 05, 20, 25)));

            Assert.That(getMappedValue(ETimeType.DuhaStart, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 06, 17, 04)));

            Assert.That(getMappedValue(ETimeType.DhuhrStart, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 13, 21, 22)));
            Assert.That(getMappedValue(ETimeType.DhuhrEnd, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 17, 25, 53)));

            Assert.That(getMappedValue(ETimeType.AsrStart, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 17, 25, 53)));
            Assert.That(getMappedValue(ETimeType.AsrEnd, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 20, 50, 59)));
            Assert.That(getMappedValue(ETimeType.AsrMithlayn, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 18, 33, 27)));
            Assert.That(getMappedValue(ETimeType.AsrKaraha, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 20, 17, 15)));

            Assert.That(getMappedValue(ETimeType.MaghribStart, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 20, 50, 59)));
            Assert.That(getMappedValue(ETimeType.MaghribEnd, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 22, 13, 17)));
            Assert.That(getMappedValue(ETimeType.MaghribIshtibaq, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 21, 41, 46)));

            Assert.That(getMappedValue(ETimeType.IshaStart, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 30, 22, 44, 14)));
            Assert.That(getMappedValue(ETimeType.IshaEnd, timeTypeByCalculationPrayerTimes), Is.EqualTo(new DateTime(2023, 7, 31, 04, 02, 30)));
        }

        private DateTime getMappedValue(ETimeType timeType, IDictionary<ETimeType, MuwaqqitPrayerTimes> timeTypeByCalculationPrayerTimes)
        {
            return timeTypeByCalculationPrayerTimes[timeType].GetDateTimeForTimeType(timeType).WithoutFractionsOfSeconds();
        }
    }
}