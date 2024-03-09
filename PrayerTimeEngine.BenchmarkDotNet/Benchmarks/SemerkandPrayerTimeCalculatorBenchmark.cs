﻿using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
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
    [MemoryDiagnoser]
    public class SemerkandPrayerTimeCalculatorBenchmark
    {
        #region data

        private static readonly LocalDate _localDate = new(2023, 7, 29);

        private static readonly List<GenericSettingConfiguration> _configs =
            [
                new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = ECalculationSource.Semerkand }
            ];

        private static readonly SemerkandLocationData _locationData =
            new()
            {
                CountryName = "Avusturya",
                CityName = "Innsbruck",
                TimezoneName = "Europe/Vienna"
            };

        #endregion data

        private static SemerkandPrayerTimeCalculator getSemerkandPrayerTimeCalculator_DataFromDbStorage(
            AppDbContext appDbContext)
        {
            // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
            new SemerkandPrayerTimeCalculator(
                    new SemerkandDBAccess(appDbContext),
                    getPreparedSemerkandApiService(),
                    Substitute.For<IPlaceService>(),
                    Substitute.For<ILogger<SemerkandPrayerTimeCalculator>>()
                ).GetPrayerTimesAsync(_localDate, _locationData, _configs, default).GetAwaiter().GetResult();

            // throw exceptions when the calculator tries using the api
            ISemerkandApiService mockedSemerkandApiService = Substitute.For<ISemerkandApiService>();
            mockedSemerkandApiService.ReturnsForAll<Task<SemerkandPrayerTimes>>((callInfo) => throw new Exception("Don't use this!"));

            return new SemerkandPrayerTimeCalculator(
                    new SemerkandDBAccess(appDbContext),
                    mockedSemerkandApiService,
                    Substitute.For<IPlaceService>(),
                    Substitute.For<ILogger<SemerkandPrayerTimeCalculator>>()
                );
        }

        private static SemerkandPrayerTimeCalculator getSemerkandPrayerTimeCalculator_DataFromApi()
        {
            // db doesn't return any data
            var semerkandDbAccessMock = Substitute.For<ISemerkandDBAccess>();
            semerkandDbAccessMock.GetCountries(Arg.Any<CancellationToken>()).Returns([]);
            semerkandDbAccessMock.GetCitiesByCountryID(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);
            semerkandDbAccessMock.GetTimesByDateAndCityID(Arg.Any<LocalDate>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ReturnsNull<SemerkandPrayerTimes>();

            return new SemerkandPrayerTimeCalculator(
                    // returns null per default
                    semerkandDbAccessMock,
                    getPreparedSemerkandApiService(),
                    Substitute.For<IPlaceService>(),
                    Substitute.For<ILogger<SemerkandPrayerTimeCalculator>>()
                );
        }

        private static SemerkandApiService getPreparedSemerkandApiService()
        {
            static HttpResponseMessage handleRequestFunc(HttpRequestMessage request)
            {
                Stream responseStream;

                if (request.RequestUri.AbsoluteUri == SemerkandApiService.GET_COUNTRIES_URL)
                    responseStream = File.OpenRead(Path.Combine(BaseTest.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestCountriesData.txt"));
                else if (request.RequestUri.AbsoluteUri == SemerkandApiService.GET_CITIES_BY_COUNTRY_URL)
                    responseStream = File.OpenRead(Path.Combine(BaseTest.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestCityData_Austria.txt"));
                else if (request.RequestUri.AbsoluteUri == string.Format(SemerkandApiService.GET_TIMES_BY_CITY, "197", "2023"))
                    responseStream = File.OpenRead(Path.Combine(BaseTest.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestPrayerTimeData_20230729_Innsbruck.txt"));
                else
                    throw new Exception($"No response registered for URL: {request.RequestUri.AbsoluteUri}");

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(responseStream)
                };
            }

            var mockHttpMessageHandler = new MockHttpMessageHandler(handleRequestFunc);
            var httpClient = new HttpClient(mockHttpMessageHandler);

            return new SemerkandApiService(httpClient);
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

            _semerkandPrayerTimeCalculator_DataFromDbStorage = getSemerkandPrayerTimeCalculator_DataFromDbStorage(appDbContext);
            _semerkandPrayerTimeCalculator_DataFromApi = getSemerkandPrayerTimeCalculator_DataFromApi();
        }

        private static SemerkandPrayerTimeCalculator _semerkandPrayerTimeCalculator_DataFromDbStorage = null;
        private static SemerkandPrayerTimeCalculator _semerkandPrayerTimeCalculator_DataFromApi = null;

#pragma warning disable CA1822 // Mark members as static
        [Benchmark]
        public ILookup<ICalculationPrayerTimes, ETimeType> SemerkandPrayerTimeCalculator_GetDataFromDb()
        {
            var result = _semerkandPrayerTimeCalculator_DataFromDbStorage.GetPrayerTimesAsync(
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
        public ILookup<ICalculationPrayerTimes, ETimeType> SemerkandPrayerTimeCalculator_GetDataFromApi()
        {
            var result = _semerkandPrayerTimeCalculator_DataFromApi.GetPrayerTimesAsync(
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
