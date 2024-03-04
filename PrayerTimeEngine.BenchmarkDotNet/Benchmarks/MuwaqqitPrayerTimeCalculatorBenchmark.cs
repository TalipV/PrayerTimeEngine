using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.Models;
using System.Data.Common;
using PrayerTimeEngine.Core.Tests.Common;
using System.Net;
using NSubstitute.Extensions;

namespace PrayerTimeEngine.BenchmarkDotNet.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    [MemoryDiagnoser]
    public class MuwaqqitPrayerTimeCalculatorBenchmark
    {
        #region data

        private static readonly LocalDate _localDate = new(2023, 7, 30);

        private static readonly List<GenericSettingConfiguration> _configs =
            [
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
            ];

        private static readonly MuwaqqitLocationData _locationData =
            new()
            {
                Latitude = 47.2803835M,
                Longitude = 11.41337M,
                TimezoneName = "Europe/Vienna"
            };

        #endregion data

        private static MuwaqqitPrayerTimeCalculator getMuwaqqitPrayerTimeCalculator_DataFromDbStorage(
            AppDbContext appDbContext)
        {
            // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
            new MuwaqqitPrayerTimeCalculator(
                    new MuwaqqitDBAccess(appDbContext),
                    getPreparedMuwaqqitApiService(),
                    new TimeTypeAttributeService()
                ).GetPrayerTimesAsync(_localDate, _locationData, _configs, default).GetAwaiter().GetResult();

            // throw exceptions when the calculator tries using the api
            IMuwaqqitApiService mockedMuwaqqitApiService = Substitute.For<IMuwaqqitApiService>();
            mockedMuwaqqitApiService.ReturnsForAll<Task<MuwaqqitPrayerTimes>>((callInfo) => throw new Exception("Don't use this!"));

            return new MuwaqqitPrayerTimeCalculator(
                    new MuwaqqitDBAccess(appDbContext),
                    mockedMuwaqqitApiService,
                    new TimeTypeAttributeService()
                );
        }

        private static MuwaqqitPrayerTimeCalculator getMuwaqqitPrayerTimeCalculator_DataFromApi()
        {
            return new MuwaqqitPrayerTimeCalculator(
                    // returns null per default
                    Substitute.For<IMuwaqqitDBAccess>(),
                    getPreparedMuwaqqitApiService(),
                    new TimeTypeAttributeService()
                );
        }

        private static MuwaqqitApiService getPreparedMuwaqqitApiService()
        {
            Func<HttpRequestMessage, HttpResponseMessage> handleRequestFunc =
                (request) =>
                {
                    string responseText =
                        request.RequestUri.AbsoluteUri switch
                        {
                            @"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2fVienna&fa=-12&ia=3.5&isn=-8&ea=-12" => File.ReadAllText(Path.Combine(BaseTest.TEST_DATA_FILE_PATH, "MuwaqqitTestData", "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Config1.txt")),
                            @"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2fVienna&fa=-7.5&ia=4.5&isn=-12&ea=-15.5" => File.ReadAllText(Path.Combine(BaseTest.TEST_DATA_FILE_PATH, "MuwaqqitTestData", "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Config2.txt")),
                            @"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2fVienna&fa=-4.5&ia=-12&isn=-12&ea=-12" => File.ReadAllText(Path.Combine(BaseTest.TEST_DATA_FILE_PATH, "MuwaqqitTestData", "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Config3.txt")),
                            @"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2fVienna&fa=-15&ia=-12&isn=-12&ea=-12" => File.ReadAllText(Path.Combine(BaseTest.TEST_DATA_FILE_PATH, "MuwaqqitTestData", "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Config4.txt")),
                            _ => throw new Exception($"No response registered for URL: {request.RequestUri.AbsoluteUri}")
                        };

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(responseText)
                    };
                };

            var mockHttpMessageHandler = new MockHttpMessageHandler(handleRequestFunc);
            var httpClient = new HttpClient(mockHttpMessageHandler);

            return new MuwaqqitApiService(httpClient);
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

            _muwaqqitPrayerTimeCalculator_DataFromDbStorage = getMuwaqqitPrayerTimeCalculator_DataFromDbStorage(appDbContext);
            _muwaqqitPrayerTimeCalculator_DataFromApi = getMuwaqqitPrayerTimeCalculator_DataFromApi();
        }

        private static MuwaqqitPrayerTimeCalculator _muwaqqitPrayerTimeCalculator_DataFromDbStorage = null;
        private static MuwaqqitPrayerTimeCalculator _muwaqqitPrayerTimeCalculator_DataFromApi = null;

        [Benchmark]
        public static ILookup<ICalculationPrayerTimes, ETimeType> MuwaqqitPrayerTimeCalculator_GetDataFromDb()
        {
            var result = _muwaqqitPrayerTimeCalculator_DataFromDbStorage.GetPrayerTimesAsync(
                _localDate,
                locationData: _locationData,
                configurations: _configs, 
                cancellationToken: default).GetAwaiter().GetResult();

            //if (result.SelectMany(x => x.ToList()).Count() != 16)
            //{
            //    throw new Exception("No, no, no. Your benchmark is not working.");
            //}

            return result;
        }

        [Benchmark]
        public static ILookup<ICalculationPrayerTimes, ETimeType> MuwaqqitPrayerTimeCalculator_GetDataFromApi()
        {
            var result = _muwaqqitPrayerTimeCalculator_DataFromApi.GetPrayerTimesAsync(
                _localDate,
                locationData: _locationData,
                configurations: _configs, 
                cancellationToken: default).GetAwaiter().GetResult();

            //if (result.SelectMany(x => x.ToList()).Count() != 16)
            //{
            //    throw new Exception("No, no, no. Your benchmark is not working.");
            //}

            return result;
        }
    }

}
