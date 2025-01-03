using BenchmarkDotNet.Attributes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NSubstitute;
using NSubstitute.Extensions;
using NSubstitute.ReturnsExtensions;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.DTOs;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.Entities;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using System.Data.Common;

namespace PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser(false)]
public class MyMosqMosquePrayerTimeProviderBenchmark
{
    #region data

    private static readonly LocalDate _localDate = new (2024, 8, 30);
    private static readonly string _externalID = "1239";

    #endregion data

    private static MyMosqMosquePrayerTimeProvider getMyMosqMosquePrayerTimeProvider_DataFromDbStorage(
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
        new MyMosqMosquePrayerTimeProvider(
                new MyMosqDBAccess(dbContextFactory),
                SubstitutionHelper.GetMockedMyMosqApiService()
            ).GetPrayerTimesAsync(
                _localDate, 
                _externalID, 
                default).GetAwaiter().GetResult();

        // throw exceptions when the calculator tries using the api
        IMyMosqApiService mockedMyMosqApiService = Substitute.For<IMyMosqApiService>();
        mockedMyMosqApiService.ReturnsForAll<Task<MawaqitResponseDTO>>((callInfo) => throw new Exception("Don't use this!"));

        return new MyMosqMosquePrayerTimeProvider(
                new MyMosqDBAccess(dbContextFactory),
                mockedMyMosqApiService
            );
    }

    private static MyMosqMosquePrayerTimeProvider getMyMosqMosquePrayerTimeProvider_DataFromApi()
    {
        // db doesn't return any data
        var myMosqDBAccessMock = Substitute.For<IMyMosqDBAccess>();
        myMosqDBAccessMock.GetPrayerTimesAsync(
            Arg.Any<LocalDate>(), 
            Arg.Any<string>(), 
            Arg.Any<CancellationToken>()).ReturnsNull<MyMosqMosqueDailyPrayerTimes>();

        return new MyMosqMosquePrayerTimeProvider(
                myMosqDBAccessMock,
                SubstitutionHelper.GetMockedMyMosqApiService()
            );
    }

    private static SqliteConnection _dbContextKeepAliveSqlConnection;

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

        _myMosqMosquePrayerTimeProvider_DataFromDbStorage = getMyMosqMosquePrayerTimeProvider_DataFromDbStorage(dbContextFactoryMock);
        _myMosqMosquePrayerTimeProvider_DataFromApi = getMyMosqMosquePrayerTimeProvider_DataFromApi();
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

    private static MyMosqMosquePrayerTimeProvider _myMosqMosquePrayerTimeProvider_DataFromDbStorage = null;
    private static MyMosqMosquePrayerTimeProvider _myMosqMosquePrayerTimeProvider_DataFromApi = null;

    [Benchmark]
    public IMosqueDailyPrayerTimes MyMosqMosquePrayerTimeProvider_GetDataFromDb()
    {
        var result = _myMosqMosquePrayerTimeProvider_DataFromDbStorage.GetPrayerTimesAsync(
            date: _localDate,
            externalID: _externalID,
            cancellationToken: default).GetAwaiter().GetResult();

        return result;
    }

    [Benchmark]
    public IMosqueDailyPrayerTimes MyMosqMosquePrayerTimeProvider_GetDataFromApi()
    {
        IMosqueDailyPrayerTimes result = _myMosqMosquePrayerTimeProvider_DataFromApi.GetPrayerTimesAsync(
            date: _localDate,
            externalID: _externalID,
            cancellationToken: default).GetAwaiter().GetResult();

        return result;
    }
}
