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
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute.ReturnsExtensions;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.Entities;
using PrayerTimeEngine.Core.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PrayerTimeEngine.BenchmarkDotNet.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    [MemoryDiagnoser(false)]
    public class FaziletPrayerTimeCalculatorBenchmark
    {
        #region data

        private static readonly ZonedDateTime _zonedDateTime = new LocalDate(2023, 7, 29).AtStartOfDayInZone(TestDataHelper.EUROPE_VIENNA_TIME_ZONE);

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
            IDbContextFactory<AppDbContext> dbContextFactory)
        {
            // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
            new FaziletPrayerTimeCalculator(
                    new FaziletDBAccess(dbContextFactory),
                    SubstitutionHelper.GetMockedFaziletApiService(),
                    Substitute.For<IPlaceService>(),
                    Substitute.For<ILogger<FaziletPrayerTimeCalculator>>()
                ).GetPrayerTimesAsync(_zonedDateTime, _locationData, _configs, default).GetAwaiter().GetResult();

            // throw exceptions when the calculator tries using the api
            IFaziletApiService mockedFaziletApiService = Substitute.For<IFaziletApiService>();
            mockedFaziletApiService.ReturnsForAll<Task<FaziletPrayerTimes>>((callInfo) => throw new Exception("Don't use this!"));

            return new FaziletPrayerTimeCalculator(
                    new FaziletDBAccess(dbContextFactory),
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
            faziletDbAccessMock.GetTimesByDateAndCityID(Arg.Any<ZonedDateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ReturnsNull<FaziletPrayerTimes>();

            return new FaziletPrayerTimeCalculator(
                    // returns null per default
                    faziletDbAccessMock,
                    SubstitutionHelper.GetMockedFaziletApiService(),
                    Substitute.For<IPlaceService>(),
                    Substitute.For<ILogger<FaziletPrayerTimeCalculator>>()
                );
        }

        private static DbConnection _dbContextKeepAliveSqlConnection;

        [GlobalSetup]
        public static void Setup()
        {

            var dbOptions = new DbContextOptionsBuilder()
                .UseSqlite($"Data Source=:memory:")
                .Options;

            var mockableDbContext =
                Substitute.ForPartsOf<AppDbContext>(
                    dbOptions,
                    new AppDbContextMetaData(),
                    Substitute.For<ISystemInfoService>());

            var mockableDbContextDatabase = Substitute.ForPartsOf<DatabaseFacade>(mockableDbContext);
            mockableDbContext.Configure().Database.Returns(mockableDbContextDatabase);
            _dbContextKeepAliveSqlConnection = mockableDbContext.Database.GetDbConnection();
            _dbContextKeepAliveSqlConnection.Open();
            mockableDbContext.Database.EnsureCreated();

            var dbContextFactoryMock = Substitute.For<IDbContextFactory<AppDbContext>>();
            dbContextFactoryMock.CreateDbContext().Returns(mockableDbContext);
            dbContextFactoryMock.CreateDbContextAsync().Returns(callInfo => Task.FromResult(mockableDbContext));

            _faziletPrayerTimeCalculator_DataFromDbStorage = getFaziletPrayerTimeCalculator_DataFromDbStorage(dbContextFactoryMock);
            _faziletPrayerTimeCalculator_DataFromApi = getFaziletPrayerTimeCalculator_DataFromApi();
        }

        private static FaziletPrayerTimeCalculator _faziletPrayerTimeCalculator_DataFromDbStorage = null;
        private static FaziletPrayerTimeCalculator _faziletPrayerTimeCalculator_DataFromApi = null;

#pragma warning disable CA1822 // Mark members as static
        [Benchmark]
        public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> FaziletPrayerTimeCalculator_GetDataFromDb()
        {
            var result = _faziletPrayerTimeCalculator_DataFromDbStorage.GetPrayerTimesAsync(
                _zonedDateTime,
                locationData: _locationData,
                configurations: _configs, 
                cancellationToken: default).GetAwaiter().GetResult();

            if (result.Count != 1)
            {
                throw new Exception("No, no, no. Your benchmark is not working.");
            }

            return result;
        }

        [Benchmark]
        public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> FaziletPrayerTimeCalculator_GetDataFromApi()
        {
            var result = _faziletPrayerTimeCalculator_DataFromApi.GetPrayerTimesAsync(
                _zonedDateTime,
                locationData: _locationData,
                configurations: _configs, 
                cancellationToken: default).GetAwaiter().GetResult();

            if (result.Count != 1)
            {
                throw new Exception("No, no, no. Your benchmark is not working.");
            }

            return result;
        }
#pragma warning restore CA1822 // Mark members as static
    }

}
