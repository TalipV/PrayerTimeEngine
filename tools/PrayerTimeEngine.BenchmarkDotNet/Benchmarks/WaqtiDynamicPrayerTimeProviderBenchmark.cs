using BenchmarkDotNet.Attributes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NSubstitute;
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.DTOs;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Services;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser]
public class WaqtiDynamicPrayerTimeProviderBenchmark
{
    #region data

    private static readonly ZonedDateTime s_zonedDateTime = new LocalDate(2023, 7, 30).AtStartOfDayInZone(TestDataHelper.EUROPE_VIENNA_TIME_ZONE);

    private static readonly List<GenericSettingConfiguration> s_configs =
        [
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrStart, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrEnd, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Method = "Mwl2007", Degree = -7.5 },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Method = "Mwl2007", Degree = -4.5 },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.DuhaStart, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrStart, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrEnd, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.AsrStart, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.AsrEnd, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.AsrMithlayn, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.AsrKaraha, Method = "Mwl2007", Degree = 4.5 },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.MaghribIshtibak, Method = "Mwl2007", Degree = -10 },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.MaghribStart, Method = "Diyanet" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.MaghribEnd, Method = "Diyanet" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.IshaStart, Method = "Diyanet" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.IshaEnd, Method = "Diyanet" },
        ];

    private static readonly WaqtiLocationData s_locationData =
        new()
        {
            Latitude = 47.2692124M,
            Longitude = 11.4041024M,
            TimezoneName = TestDataHelper.EUROPE_VIENNA_TIME_ZONE.Id
        };

    #endregion data

    private static WaqtiDynamicPrayerTimeProvider GetWaqtiDynamicPrayerTimeProvider_DataFromDbStorage(
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
        new WaqtiDynamicPrayerTimeProvider(
                new WaqtiRepository(dbContextFactory),
                SubstitutionHelper.GetMockedWaqtiApiService()
            ).GetPrayerTimesAsync(s_zonedDateTime, s_locationData, s_configs, default).GetAwaiter().GetResult();

        // throw exceptions when the provider tries using the api
        IWaqtiApiService mockedWaqtiApiService = Substitute.For<IWaqtiApiService>();
        mockedWaqtiApiService.ReturnsForAll<Task<List<WaqtiDailyPrayerTimesResponseDTO>>>((callInfo) => throw new Exception("Don't use this!"));

        return new WaqtiDynamicPrayerTimeProvider(
                new WaqtiRepository(dbContextFactory),
                mockedWaqtiApiService
            );
    }

    private static WaqtiDynamicPrayerTimeProvider GetWaqtiDynamicPrayerTimeProvider_DataFromApi()
    {
        return new WaqtiDynamicPrayerTimeProvider(
                // returns null per default
                Substitute.For<IWaqtiRepository>(),
                SubstitutionHelper.GetMockedWaqtiApiService()
            );
    }

    private static SqliteConnection s_dbContextKeepAliveSqlConnection = null!;

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

        s_waqtiDynamicPrayerTimeProvider_DataFromDbStorage = GetWaqtiDynamicPrayerTimeProvider_DataFromDbStorage(dbContextFactoryMock);
        s_waqtiDynamicPrayerTimeProvider_DataFromApi = GetWaqtiDynamicPrayerTimeProvider_DataFromApi();
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

    private static WaqtiDynamicPrayerTimeProvider s_waqtiDynamicPrayerTimeProvider_DataFromDbStorage = null!;
    private static WaqtiDynamicPrayerTimeProvider s_waqtiDynamicPrayerTimeProvider_DataFromApi = null!;

    [Benchmark]
    public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> WaqtiDynamicPrayerTimeProvider_GetDataFromDb()
    {
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result = s_waqtiDynamicPrayerTimeProvider_DataFromDbStorage.GetPrayerTimesAsync(
            s_zonedDateTime,
            locationData: s_locationData,
            configurations: s_configs,
            cancellationToken: default).GetAwaiter().GetResult();

        if (result.Count != 16)
        {
            throw new Exception("No, no, no. Your benchmark is not working.");
        }

        return result;
    }

    [Benchmark]
    public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> WaqtiDynamicPrayerTimeProvider_GetDataFromApi()
    {
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result = s_waqtiDynamicPrayerTimeProvider_DataFromApi.GetPrayerTimesAsync(
            s_zonedDateTime,
            locationData: s_locationData,
            configurations: s_configs,
            cancellationToken: default).GetAwaiter().GetResult();

        if (result.Count != 16)
        {
            throw new Exception("No, no, no. Your benchmark is not working.");
        }

        return result;
    }
}
