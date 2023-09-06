using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.SQLite;
using PrayerTimeEngine.Core.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.PlacesService.Interfaces;
using PrayerTimeEngineUnitTests.Mocks;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.API.SemerkandAPI
{
    public class SemerkandPrayerTimeCalculatorTests
    {
        private static ServiceProvider _serviceProvider = null;

        public static ServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                {
                    var serviceCollection = new ServiceCollection();

                    serviceCollection.AddSingleton(Substitute.For<ILocationService>());

                    serviceCollection.AddSingleton(Substitute.For<ILogger<SQLiteDB>>());
                    serviceCollection.AddSingleton<ISQLiteDB, SQLiteDB>();

                    serviceCollection.AddSingleton<ISemerkandDBAccess, SemerkandDBAccess>();
                    serviceCollection.AddSingleton<ISemerkandApiService>(getMockedSemerkandApiService());
                    serviceCollection.AddSingleton(Substitute.For<ILogger<SemerkandPrayerTimeCalculator>>());
                    serviceCollection.AddSingleton<SemerkandPrayerTimeCalculator>();

                    _serviceProvider = serviceCollection.BuildServiceProvider();
                }

                return _serviceProvider;
            }
        }

        private static SemerkandApiService getMockedSemerkandApiService()
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
        public async Task SemerkandPrayerTimeCalculator_GetPrayerTimesAsyncWithNormalInput_PrayerTimesForThatDay()
        {
            // ARRANGE
            SQLiteDB sqLiteDb = ServiceProvider.GetService<ISQLiteDB>() as SQLiteDB;

            using (sqLiteDb.GetSqliteConnection("Data Source=:memory:"))
            {
                sqLiteDb.InitializeDatabase(filePathDatabase: false);

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
                        new List<GenericSettingConfiguration> { new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = ECalculationSource.Semerkand } }
                    )).Single().Key;

                SemerkandPrayerTimes semerkandPrayerTimes = result as SemerkandPrayerTimes;

                // ASSERT
                Assert.IsNotNull(semerkandPrayerTimes);

                Assert.That(semerkandPrayerTimes.Date, Is.EqualTo(new LocalDate(2023, 7, 29)));
                Assert.That(semerkandPrayerTimes.Fajr.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 29, 03, 15, 0)));
                Assert.That(semerkandPrayerTimes.NextFajr.Value.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 30, 03, 17, 0)));

                Assert.That(semerkandPrayerTimes.Shuruq.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 29, 05, 41, 0)));
                Assert.That(semerkandPrayerTimes.Dhuhr.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 29, 13, 26, 0)));
                Assert.That(semerkandPrayerTimes.Asr.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 29, 17, 30, 0)));
                Assert.That(semerkandPrayerTimes.Maghrib.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 29, 21, 00, 0)));
                Assert.That(semerkandPrayerTimes.Isha.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 29, 23, 02, 0)));
            }
        }
    }
}