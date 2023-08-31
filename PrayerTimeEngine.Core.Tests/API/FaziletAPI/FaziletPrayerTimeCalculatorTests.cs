using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.SQLite;
using PrayerTimeEngine.Core.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.PlacesService.Interfaces;
using PrayerTimeEngineUnitTests.Mocks;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.API.FaziletAPI
{
    public class FaziletPrayerTimeCalculatorTests
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

                    serviceCollection.AddSingleton<IFaziletDBAccess, FaziletDBAccess>();
                    serviceCollection.AddSingleton<IFaziletApiService>(getMockedFaziletApiService());
                    serviceCollection.AddSingleton(Substitute.For<ILogger<FaziletPrayerTimeCalculator>>());
                    serviceCollection.AddSingleton<FaziletPrayerTimeCalculator>();

                    _serviceProvider = serviceCollection.BuildServiceProvider();
                }

                return _serviceProvider;
            }
        }

        private static FaziletApiService getMockedFaziletApiService()
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
            SQLiteDB sqLiteDb = ServiceProvider.GetService<ISQLiteDB>() as SQLiteDB;

            using (sqLiteDb.GetSqliteConnection("Data Source=:memory:"))
            {
                sqLiteDb.InitializeDatabase(createDatabaseIfNotExist: false);

                FaziletPrayerTimeCalculator faziletPrayerTimeCalculator = ServiceProvider.GetService<FaziletPrayerTimeCalculator>();

                // ACT
                ICalculationPrayerTimes result =
                    faziletPrayerTimeCalculator.GetPrayerTimesAsync(
                        new LocalDate(2023, 7, 29),
                        new FaziletLocationData { CountryName = "Avusturya", CityName = "Innsbruck" },
                        new List<GenericSettingConfiguration> { new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = ECalculationSource.Fazilet } }
                    ).GetAwaiter().GetResult().Single().Key;

                FaziletPrayerTimes faziletPrayerTimes = result as FaziletPrayerTimes;

                // ASSERT
                Assert.IsNotNull(faziletPrayerTimes);

                Assert.That(faziletPrayerTimes.Date, Is.EqualTo(new LocalDate(2023, 7, 29)));
                Assert.That(faziletPrayerTimes.Imsak.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 29, 03, 04, 0)));
                Assert.That(faziletPrayerTimes.Fajr.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 29, 03, 24, 0)));
                Assert.That(faziletPrayerTimes.NextFajr.Value.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 30, 03, 27, 0)));

                Assert.That(faziletPrayerTimes.Shuruq.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 29, 05, 43, 0)));
                Assert.That(faziletPrayerTimes.Dhuhr.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 29, 13, 28, 0)));
                Assert.That(faziletPrayerTimes.Asr.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 29, 17, 31, 0)));
                Assert.That(faziletPrayerTimes.Maghrib.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 29, 21, 02, 0)));
                Assert.That(faziletPrayerTimes.Isha.LocalDateTime, Is.EqualTo(new LocalDateTime(2023, 7, 29, 23, 11, 0)));
            }
        }
    }
}