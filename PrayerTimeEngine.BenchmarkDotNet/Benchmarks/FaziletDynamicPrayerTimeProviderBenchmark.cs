using BenchmarkDotNet.Attributes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using NSubstitute.Extensions;
using NSubstitute.ReturnsExtensions;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using System.Data.Common;

namespace PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser(false)]
public class FaziletDynamicPrayerTimeProviderBenchmark
{
    #region data

    private static readonly ZonedDateTime _zonedDateTime = new LocalDate(2023, 7, 29).AtStartOfDayInZone(TestDataHelper.EUROPE_VIENNA_TIME_ZONE);

    private static readonly List<GenericSettingConfiguration> _configs =
        [
            new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = EDynamicPrayerTimeProviderType.Fazilet }
        ];

    private static readonly FaziletLocationData _locationData =
        new()
        {
            CountryName = "Avusturya",
            CityName = "Innsbruck"
        };

    #endregion data

    private static FaziletDynamicPrayerTimeProvider getFaziletDynamicPrayerTimeProvider_DataFromDbStorage(
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
        new FaziletDynamicPrayerTimeProvider(
                new FaziletDBAccess(dbContextFactory),
                SubstitutionHelper.GetMockedFaziletApiService(),
                Substitute.For<IPlaceService>(),
                Substitute.For<ILogger<FaziletDynamicPrayerTimeProvider>>()
            ).GetPrayerTimesAsync(_zonedDateTime, _locationData, _configs, default).GetAwaiter().GetResult();

        // throw exceptions when the calculator tries using the api
        IFaziletApiService mockedFaziletApiService = Substitute.For<IFaziletApiService>();
        mockedFaziletApiService.ReturnsForAll<Task<FaziletDailyPrayerTimes>>((callInfo) => throw new Exception("Don't use this!"));

        return new FaziletDynamicPrayerTimeProvider(
                new FaziletDBAccess(dbContextFactory),
                mockedFaziletApiService,
                Substitute.For<IPlaceService>(),
                Substitute.For<ILogger<FaziletDynamicPrayerTimeProvider>>()
            );
    }

    private static FaziletDynamicPrayerTimeProvider getFaziletDynamicPrayerTimeProvider_DataFromApi()
    {
        // db doesn't return any data
        var faziletDbAccessMock = Substitute.For<IFaziletDBAccess>();
        faziletDbAccessMock.GetCountries(Arg.Any<CancellationToken>()).Returns([]);
        faziletDbAccessMock.GetCitiesByCountryID(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);
        faziletDbAccessMock.GetTimesByDateAndCityID(Arg.Any<ZonedDateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ReturnsNull<FaziletDailyPrayerTimes>();

        return new FaziletDynamicPrayerTimeProvider(
                // returns null per default
                faziletDbAccessMock,
                SubstitutionHelper.GetMockedFaziletApiService(),
                Substitute.For<IPlaceService>(),
                Substitute.For<ILogger<FaziletDynamicPrayerTimeProvider>>()
            );
    }

    private static DbConnection _dbContextKeepAliveSqlConnection;

    [GlobalSetup]
    public static void Setup()
    {
        _dbContextKeepAliveSqlConnection = new SqliteConnection("Data Source=:memory:");
        _dbContextKeepAliveSqlConnection.Open();

        // Create the initial DbContext to initialize the database schema
        var dbContext = getDbContext();
        dbContext.Database.EnsureCreated();

        var dbContextFactoryMock = Substitute.For<IDbContextFactory<AppDbContext>>();
        dbContextFactoryMock.CreateDbContext().Returns(callInfo => getDbContext());
        dbContextFactoryMock.CreateDbContextAsync().Returns(callInfo => Task.FromResult(getDbContext()));

        _faziletDynamicPrayerTimeProvider_DataFromDbStorage = getFaziletDynamicPrayerTimeProvider_DataFromDbStorage(dbContextFactoryMock);
        _faziletDynamicPrayerTimeProvider_DataFromApi = getFaziletDynamicPrayerTimeProvider_DataFromApi();
    }

    private static AppDbContext getDbContext()
    {
        var dbOptions = new DbContextOptionsBuilder()
            .UseSqlite(_dbContextKeepAliveSqlConnection) // Use the existing connection
            .Options;

        var dbContext =
            new AppDbContext(
                dbOptions,
                new AppDbContextMetaData(),
                Substitute.For<ISystemInfoService>());

        return dbContext;
    }

    private static FaziletDynamicPrayerTimeProvider _faziletDynamicPrayerTimeProvider_DataFromDbStorage = null;
    private static FaziletDynamicPrayerTimeProvider _faziletDynamicPrayerTimeProvider_DataFromApi = null;

#pragma warning disable CA1822 // Mark members as static
    [Benchmark]
    public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> FaziletDynamicPrayerTimeProvider_GetDataFromDb()
    {
        var result = _faziletDynamicPrayerTimeProvider_DataFromDbStorage.GetPrayerTimesAsync(
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
    public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> FaziletDynamicPrayerTimeProvider_GetDataFromApi()
    {
        var result = _faziletDynamicPrayerTimeProvider_DataFromApi.GetPrayerTimesAsync(
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
