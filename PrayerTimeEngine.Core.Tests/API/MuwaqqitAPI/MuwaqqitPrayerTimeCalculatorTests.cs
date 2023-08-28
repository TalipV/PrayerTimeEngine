using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Common.Extension;
using PrayerTimeEngine.Core.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Data.SQLite;
using PrayerTimeEngine.Domain;
using PrayerTimeEngineUnitTests.Mocks;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.API.MuwaqqitAPI
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
            List<GenericSettingConfiguration> configs =
                new()
                {
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
                };

            var muwaqqitDBAccess = new MuwaqqitDBAccess(new SQLiteDB(null), null);
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
                    new MuwaqqitLocationData { Latitude = 47.2803835M, Longitude = 11.41337M },
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