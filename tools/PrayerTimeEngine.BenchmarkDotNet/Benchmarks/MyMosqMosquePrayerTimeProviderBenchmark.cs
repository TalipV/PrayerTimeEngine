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
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser(false)]
public class MyMosqMosquePrayerTimeProviderBenchmark
{
    #region data

    private static readonly LocalDate s_localDate = new(2024, 8, 30);
    private static readonly string s_externalID = "1239";

    #endregion data

    private static MyMosqMosquePrayerTimeProvider GetMyMosqMosquePrayerTimeProvider_DataFromDbStorage(
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
        new MyMosqMosquePrayerTimeProvider(
                new MyMosqRepository(dbContextFactory),
                SubstitutionHelper.GetMockedMyMosqApiService()
            ).GetPrayerTimesAsync(
                s_localDate,
                s_externalID,
                default).GetAwaiter().GetResult();

        // throw exceptions when the calculator tries using the api
        IMyMosqApiService mockedMyMosqApiService = Substitute.For<IMyMosqApiService>();
        mockedMyMosqApiService.ReturnsForAll<Task<MawaqitResponseDTO>>((callInfo) => throw new Exception("Don't use this!"));

        return new MyMosqMosquePrayerTimeProvider(
                new MyMosqRepository(dbContextFactory),
                mockedMyMosqApiService
            );
    }

    private static MyMosqMosquePrayerTimeProvider GetMyMosqMosquePrayerTimeProvider_DataFromApi()
    {
        // db doesn't return any data
        IMyMosqRepository myMosqRepositoryMock = Substitute.For<IMyMosqRepository>();
        myMosqRepositoryMock.GetPrayerTimesAsync(
            Arg.Any<LocalDate>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()).ReturnsNull();

        return new MyMosqMosquePrayerTimeProvider(
                myMosqRepositoryMock,
                SubstitutionHelper.GetMockedMyMosqApiService()
            );
    }

    private static SqliteConnection s_dbContextKeepAliveSqlConnection;

    [GlobalSetup]
    public static void Setup()
    {
        s_dbContextKeepAliveSqlConnection = new SqliteConnection("Data Source=:memory:");
        s_dbContextKeepAliveSqlConnection.Open();

        // Create the initial DbContext to initialize the database schema
        AppDbContext dbContext = GetDbContext();
        dbContext.Database.EnsureCreated();

        IDbContextFactory<AppDbContext> dbContextFactoryMock = Substitute.For<IDbContextFactory<AppDbContext>>();
        dbContextFactoryMock.CreateDbContext().Returns(callInfo => GetDbContext());
        dbContextFactoryMock.CreateDbContextAsync().Returns(callInfo => Task.FromResult(GetDbContext()));

        s_myMosqMosquePrayerTimeProvider_DataFromDbStorage = GetMyMosqMosquePrayerTimeProvider_DataFromDbStorage(dbContextFactoryMock);
        s_myMosqMosquePrayerTimeProvider_DataFromApi = GetMyMosqMosquePrayerTimeProvider_DataFromApi();
    }

    private static AppDbContext GetDbContext()
    {
        DbContextOptions dbOptions = new DbContextOptionsBuilder()
            .UseSqlite(s_dbContextKeepAliveSqlConnection) // Use the existing connection
            .Options;

        var dbContext =
            new AppDbContext(
                dbOptions,
                new AppDbContextMetaData(),
                Substitute.For<ISystemInfoService>());

        return dbContext;
    }

    private static MyMosqMosquePrayerTimeProvider s_myMosqMosquePrayerTimeProvider_DataFromDbStorage = null;
    private static MyMosqMosquePrayerTimeProvider s_myMosqMosquePrayerTimeProvider_DataFromApi = null;

    [Benchmark]
    public IMosqueDailyPrayerTimes MyMosqMosquePrayerTimeProvider_GetDataFromDb()
    {
        IMosqueDailyPrayerTimes result = s_myMosqMosquePrayerTimeProvider_DataFromDbStorage.GetPrayerTimesAsync(
            date: s_localDate,
            externalID: s_externalID,
            cancellationToken: default).GetAwaiter().GetResult();

        return result;
    }

    [Benchmark]
    public IMosqueDailyPrayerTimes MyMosqMosquePrayerTimeProvider_GetDataFromApi()
    {
        IMosqueDailyPrayerTimes result = s_myMosqMosquePrayerTimeProvider_DataFromApi.GetPrayerTimesAsync(
            date: s_localDate,
            externalID: s_externalID,
            cancellationToken: default).GetAwaiter().GetResult();

        return result;
    }
}
