using BenchmarkDotNet.Attributes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NSubstitute;
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Services;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser]
public class MuwaqqitDynamicPrayerTimeProviderBenchmark
{
    #region data

    private static readonly ZonedDateTime s_zonedDateTime = new LocalDate(2023, 7, 30).AtStartOfDayInZone(TestDataHelper.EUROPE_VIENNA_TIME_ZONE);

    private static readonly List<GenericSettingConfiguration> s_configs =
        [
            new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrStart, Degree = -12.0 },
            new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, Source = EDynamicPrayerTimeProviderType.Muwaqqit },
            new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Degree = -7.5 },
            new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Degree = -4.5 },
            new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.DuhaStart, Degree = 3.5 },
            new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = EDynamicPrayerTimeProviderType.Muwaqqit },
            new GenericSettingConfiguration { TimeType = ETimeType.DhuhrEnd, Source = EDynamicPrayerTimeProviderType.Muwaqqit },
            new GenericSettingConfiguration { TimeType = ETimeType.AsrStart, Source = EDynamicPrayerTimeProviderType.Muwaqqit },
            new GenericSettingConfiguration { TimeType = ETimeType.AsrEnd, Source = EDynamicPrayerTimeProviderType.Muwaqqit },
            new GenericSettingConfiguration { TimeType = ETimeType.AsrMithlayn, Source = EDynamicPrayerTimeProviderType.Muwaqqit },
            new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.AsrKaraha, Degree = 4.5 },
            new GenericSettingConfiguration { TimeType = ETimeType.MaghribStart, Source = EDynamicPrayerTimeProviderType.Muwaqqit },
            new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribEnd, Degree = -12.0 },
            new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribIshtibak, Degree = -8 },
            new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.IshaStart, Degree = -15.5 },
            new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.IshaEnd, Degree = -15.0 },
        ];

    private static readonly MuwaqqitLocationData s_locationData =
        new()
        {
            Latitude = 47.2803835M,
            Longitude = 11.41337M,
            TimezoneName = TestDataHelper.EUROPE_VIENNA_TIME_ZONE.Id
        };

    #endregion data

    private static MuwaqqitDynamicPrayerTimeProvider GetMuwaqqitDynamicPrayerTimeProvider_DataFromDbStorage(
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
        new MuwaqqitDynamicPrayerTimeProvider(
                new MuwaqqitRepository(dbContextFactory),
                SubstitutionHelper.GetMockedMuwaqqitApiService(),
                new TimeTypeAttributeService()
            ).GetPrayerTimesAsync(s_zonedDateTime, s_locationData, s_configs, default).GetAwaiter().GetResult();

        // throw exceptions when the calculator tries using the api
        IMuwaqqitApiService mockedMuwaqqitApiService = Substitute.For<IMuwaqqitApiService>();
        mockedMuwaqqitApiService.ReturnsForAll<Task<MuwaqqitDailyPrayerTimes>>((callInfo) => throw new Exception("Don't use this!"));

        return new MuwaqqitDynamicPrayerTimeProvider(
                new MuwaqqitRepository(dbContextFactory),
                mockedMuwaqqitApiService,
                new TimeTypeAttributeService()
            );
    }

    private static MuwaqqitDynamicPrayerTimeProvider GetMuwaqqitDynamicPrayerTimeProvider_DataFromApi()
    {
        return new MuwaqqitDynamicPrayerTimeProvider(
                // returns null per default
                Substitute.For<IMuwaqqitRepository>(),
                SubstitutionHelper.GetMockedMuwaqqitApiService(),
                new TimeTypeAttributeService()
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

        s_muwaqqitDynamicPrayerTimeProvider_DataFromDbStorage = GetMuwaqqitDynamicPrayerTimeProvider_DataFromDbStorage(dbContextFactoryMock);
        s_muwaqqitDynamicPrayerTimeProvider_DataFromApi = GetMuwaqqitDynamicPrayerTimeProvider_DataFromApi();
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

    private static MuwaqqitDynamicPrayerTimeProvider s_muwaqqitDynamicPrayerTimeProvider_DataFromDbStorage = null;
    private static MuwaqqitDynamicPrayerTimeProvider s_muwaqqitDynamicPrayerTimeProvider_DataFromApi = null;

    [Benchmark]
    public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> MuwaqqitDynamicPrayerTimeProvider_GetDataFromDb()
    {
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result = s_muwaqqitDynamicPrayerTimeProvider_DataFromDbStorage.GetPrayerTimesAsync(
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
    public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> MuwaqqitDynamicPrayerTimeProvider_GetDataFromApi()
    {
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result = s_muwaqqitDynamicPrayerTimeProvider_DataFromApi.GetPrayerTimesAsync(
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
