using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.Models;
using System.Data.Common;
using PrayerTimeEngine.Core.Tests.Common;
using System.Net;
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute.ReturnsExtensions;

namespace PrayerTimeEngine.BenchmarkDotNet.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    [MemoryDiagnoser(false)]
    public class FaziletPrayerTimeCalculatorBenchmark
    {
        #region data

        private static readonly LocalDate _localDate = new(2023, 7, 29);

        private static readonly List<GenericSettingConfiguration> _configs =
            [
                new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = ECalculationSource.Fazilet }
            ];

        private static readonly FaziletLocationData _locationData =
            new()
            {
                CountryName = "Avusturya",
                CityName = "Innsbruck"
            };

        #endregion data

        private static FaziletPrayerTimeCalculator getFaziletPrayerTimeCalculator_DataFromDbStorage(
            AppDbContext appDbContext)
        {
            // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
            new FaziletPrayerTimeCalculator(
                    new FaziletDBAccess(appDbContext),
                    getPreparedFaziletApiService(),
                    Substitute.For<IPlaceService>(),
                    Substitute.For<ILogger<FaziletPrayerTimeCalculator>>()
                ).GetPrayerTimesAsync(_localDate, _locationData, _configs, default).GetAwaiter().GetResult();

            // throw exceptions when the calculator tries using the api
            IFaziletApiService mockedFaziletApiService = Substitute.For<IFaziletApiService>();
            mockedFaziletApiService.ReturnsForAll<Task<FaziletPrayerTimes>>((callInfo) => throw new Exception("Don't use this!"));

            return new FaziletPrayerTimeCalculator(
                    new FaziletDBAccess(appDbContext),
                    mockedFaziletApiService,
                    Substitute.For<IPlaceService>(),
                    Substitute.For<ILogger<FaziletPrayerTimeCalculator>>()
                );
        }

        private static FaziletPrayerTimeCalculator getFaziletPrayerTimeCalculator_DataFromApi()
        {
            // db doesn't return any data
            var faziletDbAccessMock = Substitute.For<IFaziletDBAccess>();
            faziletDbAccessMock.GetCountries(Arg.Any<CancellationToken>()).Returns([]);
            faziletDbAccessMock.GetCitiesByCountryID(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);
            faziletDbAccessMock.GetTimesByDateAndCityID(Arg.Any<LocalDate>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ReturnsNull<FaziletPrayerTimes>();

            return new FaziletPrayerTimeCalculator(
                    // returns null per default
                    faziletDbAccessMock,
                    getPreparedFaziletApiService(),
                    Substitute.For<IPlaceService>(),
                    Substitute.For<ILogger<FaziletPrayerTimeCalculator>>()
                );
        }

        private static FaziletApiService getPreparedFaziletApiService()
        {
            string dummyBaseURL = @"http://dummy.url.com";

            HttpResponseMessage handleRequestFunc(HttpRequestMessage request)
            {
                Stream responseStream;

                if (request.RequestUri.AbsoluteUri == $@"{dummyBaseURL}/{FaziletApiService.GET_COUNTRIES_URL}")
                    responseStream = File.OpenRead(Path.Combine(BaseTest.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestCountriesData.txt"));
                else if (request.RequestUri.AbsoluteUri == $@"{dummyBaseURL}/{FaziletApiService.GET_CITIES_BY_COUNTRY_URL}2")
                    responseStream = File.OpenRead(Path.Combine(BaseTest.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestCityData_Austria.txt"));
                else if (request.RequestUri.AbsoluteUri == $@"{dummyBaseURL}/{string.Format(FaziletApiService.GET_TIMES_BY_CITY_URL, "92")}")
                    responseStream = File.OpenRead(Path.Combine(BaseTest.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestPrayerTimeData_20230729_Innsbruck.txt"));
                else
                    throw new Exception($"No response registered for URL: {request.RequestUri.AbsoluteUri}");

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(responseStream)
                };
            }

            var mockHttpMessageHandler = new MockHttpMessageHandler(handleRequestFunc);
            var httpClient = new HttpClient(mockHttpMessageHandler) { BaseAddress = new Uri(dummyBaseURL) };

            return new FaziletApiService(httpClient);
        }

        private static DbConnection _dbContextKeepAliveSqlConnection;

        [GlobalSetup]
        public static void Setup()
        {
            var dbOptions = new DbContextOptionsBuilder()
                .UseSqlite($"Data Source=:memory:")
                .Options;
            var appDbContext = new AppDbContext(dbOptions);
            _dbContextKeepAliveSqlConnection = appDbContext.Database.GetDbConnection();
            _dbContextKeepAliveSqlConnection.Open();
            appDbContext.Database.EnsureCreated();

            _faziletPrayerTimeCalculator_DataFromDbStorage = getFaziletPrayerTimeCalculator_DataFromDbStorage(appDbContext);
            _faziletPrayerTimeCalculator_DataFromApi = getFaziletPrayerTimeCalculator_DataFromApi();
        }

        private static FaziletPrayerTimeCalculator _faziletPrayerTimeCalculator_DataFromDbStorage = null;
        private static FaziletPrayerTimeCalculator _faziletPrayerTimeCalculator_DataFromApi = null;

#pragma warning disable CA1822 // Mark members as static
        [Benchmark]
        public ILookup<ICalculationPrayerTimes, ETimeType> FaziletPrayerTimeCalculator_GetDataFromDb()
        {
            var result = _faziletPrayerTimeCalculator_DataFromDbStorage.GetPrayerTimesAsync(
                _localDate,
                locationData: _locationData,
                configurations: _configs, 
                cancellationToken: default).GetAwaiter().GetResult();

            if (result.SelectMany(x => x.ToList()).Count() != 1)
            {
                throw new Exception("No, no, no. Your benchmark is not working.");
            }

            return result;
        }

        [Benchmark]
        public ILookup<ICalculationPrayerTimes, ETimeType> FaziletPrayerTimeCalculator_GetDataFromApi()
        {
            var result = _faziletPrayerTimeCalculator_DataFromApi.GetPrayerTimesAsync(
                _localDate,
                locationData: _locationData,
                configurations: _configs, 
                cancellationToken: default).GetAwaiter().GetResult();

            if (result.SelectMany(x => x.ToList()).Count() != 1)
            {
                throw new Exception("No, no, no. Your benchmark is not working.");
            }

            return result;
        }
#pragma warning restore CA1822 // Mark members as static
    }

}
