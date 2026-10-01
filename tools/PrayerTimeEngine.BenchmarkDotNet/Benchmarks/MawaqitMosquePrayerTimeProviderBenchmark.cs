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
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Services;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser(false)]
public class MawaqitMosquePrayerTimeProviderBenchmark
{
    #region data

    private static readonly LocalDate s_localDate = new(2024, 8, 29);
    private static readonly string s_externalID = "hamza-koln";

    #endregion data

    private static MawaqitMosquePrayerTimeProvider GetMawaqitMosquePrayerTimeProvider_DataFromDbStorage(
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
        new MawaqitMosquePrayerTimeProvider(
                new MawaqitRepository(dbContextFactory),
                SubstitutionHelper.GetMockedMawaqitApiService(),
                SubstitutionHelper.GetMockedSystemInfoService(new LocalDate(1996, 10, 30).AtStartOfDayInZone(DateTimeZone.Utc))
            ).GetPrayerTimesAsync(
                s_localDate,
                s_externalID,
                default).GetAwaiter().GetResult();

        // throw exceptions when the calculator tries using the api
        IMawaqitApiService mockedMawaqitApiService = Substitute.For<IMawaqitApiService>();
        mockedMawaqitApiService.ReturnsForAll<Task<MawaqitResponseDTO>>((callInfo) => throw new Exception("Don't use this!"));

        return new MawaqitMosquePrayerTimeProvider(
                new MawaqitRepository(dbContextFactory),
                mockedMawaqitApiService,
                SubstitutionHelper.GetMockedSystemInfoService(new LocalDate(1996, 10, 30).AtStartOfDayInZone(DateTimeZone.Utc))
            );
    }

    private static MawaqitMosquePrayerTimeProvider GetMawaqitMosquePrayerTimeProvider_DataFromApi()
    {
        // db doesn't return any data
        IMawaqitRepository mawaqitDbAccessMock = Substitute.For<IMawaqitRepository>();
        mawaqitDbAccessMock.GetPrayerTimesAsync(
            Arg.Any<LocalDate>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()).ReturnsNull();

        return new MawaqitMosquePrayerTimeProvider(
                mawaqitDbAccessMock,
                SubstitutionHelper.GetMockedMawaqitApiService(),
                SubstitutionHelper.GetMockedSystemInfoService(new LocalDate(1996, 10, 30).AtStartOfDayInZone(DateTimeZone.Utc))
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

        s_mawaqitMosquePrayerTimeProvider_DataFromDbStorage = GetMawaqitMosquePrayerTimeProvider_DataFromDbStorage(dbContextFactoryMock);
        s_mawaqitMosquePrayerTimeProvider_DataFromApi = GetMawaqitMosquePrayerTimeProvider_DataFromApi();
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

    private static MawaqitMosquePrayerTimeProvider s_mawaqitMosquePrayerTimeProvider_DataFromDbStorage = null;
    private static MawaqitMosquePrayerTimeProvider s_mawaqitMosquePrayerTimeProvider_DataFromApi = null;

    [Benchmark]
    public IMosqueDailyPrayerTimes MawaqitMosquePrayerTimeProvider_GetDataFromDb()
    {
        IMosqueDailyPrayerTimes result = s_mawaqitMosquePrayerTimeProvider_DataFromDbStorage.GetPrayerTimesAsync(
            date: s_localDate,
            externalID: s_externalID,
            cancellationToken: default).GetAwaiter().GetResult();

        return result;
    }

    [Benchmark]
    public IMosqueDailyPrayerTimes MawaqitMosquePrayerTimeProvider_GetDataFromApi()
    {
        IMosqueDailyPrayerTimes result = s_mawaqitMosquePrayerTimeProvider_DataFromApi.GetPrayerTimesAsync(
            date: s_localDate,
            externalID: s_externalID,
            cancellationToken: default).GetAwaiter().GetResult();

        return result;
    }
}
