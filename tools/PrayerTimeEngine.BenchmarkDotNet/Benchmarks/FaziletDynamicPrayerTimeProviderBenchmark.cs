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

namespace PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser(false)]
public class FaziletDynamicPrayerTimeProviderBenchmark
{
    #region data

    private static readonly ZonedDateTime s_zonedDateTime = new LocalDate(2023, 7, 29).AtStartOfDayInZone(TestDataHelper.EUROPE_VIENNA_TIME_ZONE);

    private static readonly List<GenericSettingConfiguration> s_configs =
        [
            new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = EDynamicPrayerTimeProviderType.Fazilet }
        ];

    private static readonly FaziletLocationData s_locationData =
        new()
        {
            CountryName = "Avusturya",
            CityName = "Innsbruck"
        };

    #endregion data

    private static FaziletDynamicPrayerTimeProvider GetFaziletDynamicPrayerTimeProvider_DataFromDbStorage(
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
        new FaziletDynamicPrayerTimeProvider(
                new FaziletRepository(dbContextFactory),
                SubstitutionHelper.GetMockedFaziletApiService(),
                Substitute.For<IPlaceService>(),
                Substitute.For<ILogger<FaziletDynamicPrayerTimeProvider>>()
            ).GetPrayerTimesAsync(s_zonedDateTime, s_locationData, s_configs, default).GetAwaiter().GetResult();

        // throw exceptions when the calculator tries using the api
        IFaziletApiService mockedFaziletApiService = Substitute.For<IFaziletApiService>();
        mockedFaziletApiService.ReturnsForAll<Task<FaziletDailyPrayerTimes>>((callInfo) => throw new Exception("Don't use this!"));

        return new FaziletDynamicPrayerTimeProvider(
                new FaziletRepository(dbContextFactory),
                mockedFaziletApiService,
                Substitute.For<IPlaceService>(),
                Substitute.For<ILogger<FaziletDynamicPrayerTimeProvider>>()
            );
    }

    private static FaziletDynamicPrayerTimeProvider GetFaziletDynamicPrayerTimeProvider_DataFromApi()
    {
        // db doesn't return any data
        IFaziletRepository faziletDbAccessMock = Substitute.For<IFaziletRepository>();
        faziletDbAccessMock.GetCountries(Arg.Any<CancellationToken>()).Returns([]);
        faziletDbAccessMock.GetCitiesByCountryID(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);
        faziletDbAccessMock.GetTimesByDateAndCityID(Arg.Any<LocalDate>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ReturnsNull();

        return new FaziletDynamicPrayerTimeProvider(
                // returns null per default
                faziletDbAccessMock,
                SubstitutionHelper.GetMockedFaziletApiService(),
                Substitute.For<IPlaceService>(),
                Substitute.For<ILogger<FaziletDynamicPrayerTimeProvider>>()
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

        s_faziletDynamicPrayerTimeProvider_DataFromDbStorage = GetFaziletDynamicPrayerTimeProvider_DataFromDbStorage(dbContextFactoryMock);
        s_faziletDynamicPrayerTimeProvider_DataFromApi = GetFaziletDynamicPrayerTimeProvider_DataFromApi();
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

    private static FaziletDynamicPrayerTimeProvider s_faziletDynamicPrayerTimeProvider_DataFromDbStorage = null;
    private static FaziletDynamicPrayerTimeProvider s_faziletDynamicPrayerTimeProvider_DataFromApi = null;

    [Benchmark]
    public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> FaziletDynamicPrayerTimeProvider_GetDataFromDb()
    {
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result = s_faziletDynamicPrayerTimeProvider_DataFromDbStorage.GetPrayerTimesAsync(
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
    public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> FaziletDynamicPrayerTimeProvider_GetDataFromApi()
    {
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result = s_faziletDynamicPrayerTimeProvider_DataFromApi.GetPrayerTimesAsync(
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
