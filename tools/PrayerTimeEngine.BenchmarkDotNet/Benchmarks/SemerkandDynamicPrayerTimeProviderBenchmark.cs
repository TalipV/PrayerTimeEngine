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
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser]
public class SemerkandDynamicPrayerTimeProviderBenchmark
{
    #region data

    private static readonly ZonedDateTime s_zonedDateTime = new LocalDate(2023, 7, 29).AtStartOfDayInZone(TestDataHelper.EUROPE_VIENNA_TIME_ZONE);

    private static readonly List<GenericSettingConfiguration> s_configs =
        [
            new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = EDynamicPrayerTimeProviderType.Semerkand }
        ];

    private static readonly SemerkandLocationData s_locationData =
        new()
        {
            CountryName = "Avusturya",
            CityName = "Innsbruck",
            TimezoneName = TestDataHelper.EUROPE_VIENNA_TIME_ZONE.Id
        };

    #endregion data

    private static SemerkandDynamicPrayerTimeProvider GetSemerkandDynamicPrayerTimeProvider_DataFromDbStorage(
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
        new SemerkandDynamicPrayerTimeProvider(
                new SemerkandRepository(dbContextFactory),
                SubstitutionHelper.GetMockedSemerkandApiService(),
                Substitute.For<IPlaceService>(),
                Substitute.For<ILogger<SemerkandDynamicPrayerTimeProvider>>()
            ).GetPrayerTimesAsync(s_zonedDateTime, s_locationData, s_configs, default).GetAwaiter().GetResult();

        // throw exceptions when the calculator tries using the api
        ISemerkandApiService mockedSemerkandApiService = Substitute.For<ISemerkandApiService>();
        mockedSemerkandApiService.ReturnsForAll<Task<SemerkandDailyPrayerTimes>>((callInfo) => throw new Exception("Don't use this!"));

        return new SemerkandDynamicPrayerTimeProvider(
                new SemerkandRepository(dbContextFactory),
                mockedSemerkandApiService,
                Substitute.For<IPlaceService>(),
                Substitute.For<ILogger<SemerkandDynamicPrayerTimeProvider>>()
            );
    }

    private static SemerkandDynamicPrayerTimeProvider GetSemerkandDynamicPrayerTimeProvider_DataFromApi()
    {
        // db doesn't return any data
        ISemerkandRepository semerkandDbAccessMock = Substitute.For<ISemerkandRepository>();
        semerkandDbAccessMock.GetCountries(Arg.Any<CancellationToken>()).Returns([]);
        semerkandDbAccessMock.GetCitiesByCountryID(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);
        semerkandDbAccessMock.GetTimesByDateAndCityID(Arg.Any<LocalDate>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ReturnsNull();

        return new SemerkandDynamicPrayerTimeProvider(
                // returns null per default
                semerkandDbAccessMock,
                SubstitutionHelper.GetMockedSemerkandApiService(),
                Substitute.For<IPlaceService>(),
                Substitute.For<ILogger<SemerkandDynamicPrayerTimeProvider>>()
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

        s_semerkandDynamicPrayerTimeProvider_DataFromDbStorage = GetSemerkandDynamicPrayerTimeProvider_DataFromDbStorage(dbContextFactoryMock);
        s_semerkandDynamicPrayerTimeProvider_DataFromApi = GetSemerkandDynamicPrayerTimeProvider_DataFromApi();
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

    private static SemerkandDynamicPrayerTimeProvider s_semerkandDynamicPrayerTimeProvider_DataFromDbStorage = null;
    private static SemerkandDynamicPrayerTimeProvider s_semerkandDynamicPrayerTimeProvider_DataFromApi = null;

    [Benchmark]
    public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> SemerkandDynamicPrayerTimeProvider_GetDataFromDb()
    {
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result = s_semerkandDynamicPrayerTimeProvider_DataFromDbStorage.GetPrayerTimesAsync(
            s_zonedDateTime,
            locationData: s_locationData,
            configurations: s_configs,
            cancellationToken: default).GetAwaiter().GetResult();

        if (result.Count != 1)
        {
            throw new Exception("No, no, no. Your benchmark is not working.");
        }

        return result;
    }

    [Benchmark]
    public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> SemerkandDynamicPrayerTimeProvider_GetDataFromApi()
    {
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result = s_semerkandDynamicPrayerTimeProvider_DataFromApi.GetPrayerTimesAsync(
            s_zonedDateTime,
            locationData: s_locationData,
            configurations: s_configs,
            cancellationToken: default).GetAwaiter().GetResult();

        if (result.Count != 1)
        {
            throw new Exception("No, no, no. Your benchmark is not working.");
        }

        return result;
    }
}
