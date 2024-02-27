using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using System.Data.Common;
using PrayerTimeEngine.Core.Tests.Common;
using System.Net;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NSubstitute.Extensions;
using System.Security;

namespace PrayerTimeEngine.BenchmarkDotNet
{
    [Config(typeof(BenchmarkConfig))]
    [MemoryDiagnoser]
    public class MuwaqqitPrayerTimeCalculatorBenchmark
    {
        #region data

        private static readonly LocalDate _localDate = new LocalDate(2023, 7, 30);

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

        private static MuwaqqitLocationData _locationData =
            new MuwaqqitLocationData
            {
                Latitude = 47.2803835M,
                Longitude = 11.41337M,
                TimezoneName = "Europe/Vienna"
            };

        #endregion data

        private static DbConnection _dbContextKeepAliveSqlConnection;
        protected AppDbContext createAppDbContextAndKeepAlive()
        {
            var dbOptions = new DbContextOptionsBuilder()
                .UseSqlite($"Data Source=:memory:")
                .Options;
            var appDbContext = Substitute.ForPartsOf<AppDbContext>(dbOptions);
            var mockableDbContextDatabase = Substitute.ForPartsOf<DatabaseFacade>(appDbContext);
            appDbContext.Configure().Database.Returns(mockableDbContextDatabase);
            var database = appDbContext.Database;
            _dbContextKeepAliveSqlConnection = database.GetDbConnection();
            _dbContextKeepAliveSqlConnection.Open();
            database.EnsureCreated();

            return appDbContext;
        }

        private MuwaqqitPrayerTimeCalculator getMuwaqqitPrayerTimeCalculator_DataFromDbStorage()
        {
            // setup basic stuff
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(Substitute.For<IPlaceService>());
            serviceCollection.AddSingleton(Substitute.For<ILogger<MuwaqqitDBAccess>>());
            serviceCollection.AddSingleton(Substitute.For<ILogger<MuwaqqitPrayerTimeCalculator>>());

            serviceCollection.AddSingleton<TimeTypeAttributeService>();
            serviceCollection.AddSingleton(createAppDbContextAndKeepAlive());
            serviceCollection.AddSingleton<IMuwaqqitApiService>(getPreparedMuwaqqitApiService());
            serviceCollection.AddSingleton<IMuwaqqitDBAccess, MuwaqqitDBAccess>();
            serviceCollection.AddTransient<MuwaqqitPrayerTimeCalculator>();

            // make sure that the data is gotten from the API service so that later it will only be retrieved from the DB
            var muwaqqitPrayerTimeCalculator = serviceCollection.BuildServiceProvider().GetService<MuwaqqitPrayerTimeCalculator>();
            muwaqqitPrayerTimeCalculator.GetPrayerTimesAsync(
                _localDate,
                _locationData,
                _configs).GetAwaiter().GetResult();

            // make sure the benchmark is correct by throwing exceptions when the api service is used
            IMuwaqqitApiService mockedMuwaqqitApiService = Substitute.For<IMuwaqqitApiService>();
            mockedMuwaqqitApiService
                .GetTimesAsync(
                    date: Arg.Any<LocalDate>(),
                    longitude: Arg.Any<decimal>(),
                    latitude: Arg.Any<decimal>(),
                    fajrDegree: Arg.Any<double>(),
                    ishaDegree: Arg.Any<double>(),
                    ishtibaqDegree: Arg.Any<double>(),
                    asrKarahaDegree: Arg.Any<double>(),
                    timezone: Arg.Any<string>())
                .Returns(returnThis: (callInfo) =>
                    {
                        throw new SecurityException("Don't reach this!");
                        return (MuwaqqitPrayerTimes)null;
                    });
            serviceCollection.AddSingleton(mockedMuwaqqitApiService);

            return serviceCollection.BuildServiceProvider().GetService<MuwaqqitPrayerTimeCalculator>();
        }

        private MuwaqqitPrayerTimeCalculator getMuwaqqitPrayerTimeCalculator_DataFromApi()
        {
            // setup basic stuff
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(Substitute.For<IPlaceService>());
            serviceCollection.AddSingleton(Substitute.For<ILogger<MuwaqqitPrayerTimeCalculator>>());

            serviceCollection.AddSingleton<TimeTypeAttributeService>();
            serviceCollection.AddSingleton<IMuwaqqitApiService>(getPreparedMuwaqqitApiService());
            serviceCollection.AddSingleton<MuwaqqitPrayerTimeCalculator>();

            // make sure the benchmark is correct by returning null and empty from the db access
            IMuwaqqitDBAccess mockedMuwaqqitDBAccess = Substitute.For<IMuwaqqitDBAccess>();
            serviceCollection.AddSingleton(mockedMuwaqqitDBAccess);

            return serviceCollection.BuildServiceProvider().GetService<MuwaqqitPrayerTimeCalculator>();
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

        [GlobalSetup]
        public void Setup()
        {
            _muwaqqitPrayerTimeCalculator_DataFromDbStorage = getMuwaqqitPrayerTimeCalculator_DataFromDbStorage();
            _muwaqqitPrayerTimeCalculator_DataFromApi = getMuwaqqitPrayerTimeCalculator_DataFromApi();
        }

        private static MuwaqqitPrayerTimeCalculator _muwaqqitPrayerTimeCalculator_DataFromDbStorage = null;
        private static MuwaqqitPrayerTimeCalculator _muwaqqitPrayerTimeCalculator_DataFromApi = null;

        [Benchmark]
        public void MuwaqqitPrayerTimeCalculator_GetDataFromDb()
        {
            _muwaqqitPrayerTimeCalculator_DataFromDbStorage.GetPrayerTimesAsync(
                _localDate,
                locationData: _locationData,
                configurations: _configs).GetAwaiter().GetResult();
        }        
        
        [Benchmark]
        public void MuwaqqitPrayerTimeCalculator_GetDataFromApi()
        {
            _muwaqqitPrayerTimeCalculator_DataFromApi.GetPrayerTimesAsync(
                _localDate,
                locationData: _locationData,
                configurations: _configs).GetAwaiter().GetResult();
        }
    }

}
