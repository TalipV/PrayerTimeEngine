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
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.DTOs;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.Entities;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Services;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser(false)]
public class MawaqitMosquePrayerTimeProviderBenchmark
{
    #region data

    private static readonly LocalDate _localDate = new(2024, 8, 29);
    private static readonly string _externalID = "hamza-koln";

    #endregion data

    private static MawaqitMosquePrayerTimeProvider getMawaqitMosquePrayerTimeProvider_DataFromDbStorage(
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
        new MawaqitMosquePrayerTimeProvider(
                new MawaqitDBAccess(dbContextFactory),
                SubstitutionHelper.GetMockedMawaqitApiService(),
                SubstitutionHelper.GetMockedSystemInfoService(new LocalDate(1996, 10, 30).AtStartOfDayInZone(DateTimeZone.Utc))
            ).GetPrayerTimesAsync(
                _localDate,
                _externalID,
                default).GetAwaiter().GetResult();

        // throw exceptions when the calculator tries using the api
        IMawaqitApiService mockedMawaqitApiService = Substitute.For<IMawaqitApiService>();
        mockedMawaqitApiService.ReturnsForAll<Task<MawaqitResponseDTO>>((callInfo) => throw new Exception("Don't use this!"));

        return new MawaqitMosquePrayerTimeProvider(
                new MawaqitDBAccess(dbContextFactory),
                mockedMawaqitApiService,
                SubstitutionHelper.GetMockedSystemInfoService(new LocalDate(1996, 10, 30).AtStartOfDayInZone(DateTimeZone.Utc))
            );
    }

    private static MawaqitMosquePrayerTimeProvider getMawaqitMosquePrayerTimeProvider_DataFromApi()
    {
        // db doesn't return any data
        var mawaqitDbAccessMock = Substitute.For<IMawaqitDBAccess>();
        mawaqitDbAccessMock.GetPrayerTimesAsync(
            Arg.Any<LocalDate>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()).ReturnsNull<MawaqitMosqueDailyPrayerTimes>();

        return new MawaqitMosquePrayerTimeProvider(
                mawaqitDbAccessMock,
                SubstitutionHelper.GetMockedMawaqitApiService(),
                SubstitutionHelper.GetMockedSystemInfoService(new LocalDate(1996, 10, 30).AtStartOfDayInZone(DateTimeZone.Utc))
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

        _mawaqitMosquePrayerTimeProvider_DataFromDbStorage = getMawaqitMosquePrayerTimeProvider_DataFromDbStorage(dbContextFactoryMock);
        _mawaqitMosquePrayerTimeProvider_DataFromApi = getMawaqitMosquePrayerTimeProvider_DataFromApi();
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

    private static MawaqitMosquePrayerTimeProvider _mawaqitMosquePrayerTimeProvider_DataFromDbStorage = null;
    private static MawaqitMosquePrayerTimeProvider _mawaqitMosquePrayerTimeProvider_DataFromApi = null;

    [Benchmark]
    public IMosqueDailyPrayerTimes MawaqitMosquePrayerTimeProvider_GetDataFromDb()
    {
        var result = _mawaqitMosquePrayerTimeProvider_DataFromDbStorage.GetPrayerTimesAsync(
            date: _localDate,
            externalID: _externalID,
            cancellationToken: default).GetAwaiter().GetResult();

        return result;
    }

    [Benchmark]
    public IMosqueDailyPrayerTimes MawaqitMosquePrayerTimeProvider_GetDataFromApi()
    {
        IMosqueDailyPrayerTimes result = _mawaqitMosquePrayerTimeProvider_DataFromApi.GetPrayerTimesAsync(
            date: _localDate,
            externalID: _externalID,
            cancellationToken: default).GetAwaiter().GetResult();

        return result;
    }
}
