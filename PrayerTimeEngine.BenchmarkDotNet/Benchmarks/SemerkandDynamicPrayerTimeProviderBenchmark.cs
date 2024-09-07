using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using System.Data.Common;
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute.ReturnsExtensions;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using PrayerTimeEngine.Core.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services;

namespace PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser]
public class SemerkandDynamicPrayerTimeProviderBenchmark
{
    #region data

    private static readonly ZonedDateTime _zonedDateTime = new LocalDate(2023, 7, 29).AtStartOfDayInZone(TestDataHelper.EUROPE_VIENNA_TIME_ZONE);

    private static readonly List<GenericSettingConfiguration> _configs =
        [
            new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = EDynamicPrayerTimeProviderType.Semerkand }
        ];

    private static readonly SemerkandLocationData _locationData =
        new()
        {
            CountryName = "Avusturya",
            CityName = "Innsbruck",
            TimezoneName = TestDataHelper.EUROPE_VIENNA_TIME_ZONE.Id
        };

    #endregion data

    private static SemerkandDynamicPrayerTimeProvider getSemerkandDynamicPrayerTimeProvider_DataFromDbStorage(
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
        new SemerkandDynamicPrayerTimeProvider(
                new SemerkandDBAccess(dbContextFactory),
                SubstitutionHelper.GetMockedSemerkandApiService(),
                Substitute.For<IPlaceService>(),
                Substitute.For<ILogger<SemerkandDynamicPrayerTimeProvider>>()
            ).GetPrayerTimesAsync(_zonedDateTime, _locationData, _configs, default).GetAwaiter().GetResult();

        // throw exceptions when the calculator tries using the api
        ISemerkandApiService mockedSemerkandApiService = Substitute.For<ISemerkandApiService>();
        mockedSemerkandApiService.ReturnsForAll<Task<SemerkandDailyPrayerTimes>>((callInfo) => throw new Exception("Don't use this!"));

        return new SemerkandDynamicPrayerTimeProvider(
                new SemerkandDBAccess(dbContextFactory),
                mockedSemerkandApiService,
                Substitute.For<IPlaceService>(),
                Substitute.For<ILogger<SemerkandDynamicPrayerTimeProvider>>()
            );
    }

    private static SemerkandDynamicPrayerTimeProvider getSemerkandDynamicPrayerTimeProvider_DataFromApi()
    {
        // db doesn't return any data
        var semerkandDbAccessMock = Substitute.For<ISemerkandDBAccess>();
        semerkandDbAccessMock.GetCountries(Arg.Any<CancellationToken>()).Returns([]);
        semerkandDbAccessMock.GetCitiesByCountryID(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);
        semerkandDbAccessMock.GetTimesByDateAndCityID(Arg.Any<ZonedDateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ReturnsNull<SemerkandDailyPrayerTimes>();

        return new SemerkandDynamicPrayerTimeProvider(
                // returns null per default
                semerkandDbAccessMock,
                SubstitutionHelper.GetMockedSemerkandApiService(),
                Substitute.For<IPlaceService>(),
                Substitute.For<ILogger<SemerkandDynamicPrayerTimeProvider>>()
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

        _semerkandDynamicPrayerTimeProvider_DataFromDbStorage = getSemerkandDynamicPrayerTimeProvider_DataFromDbStorage(dbContextFactoryMock);
        _semerkandDynamicPrayerTimeProvider_DataFromApi = getSemerkandDynamicPrayerTimeProvider_DataFromApi();
    }

    private static SemerkandDynamicPrayerTimeProvider _semerkandDynamicPrayerTimeProvider_DataFromDbStorage = null;
    private static SemerkandDynamicPrayerTimeProvider _semerkandDynamicPrayerTimeProvider_DataFromApi = null;

#pragma warning disable CA1822 // Mark members as static
    [Benchmark]
    public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> SemerkandDynamicPrayerTimeProvider_GetDataFromDb()
    {
        var result = _semerkandDynamicPrayerTimeProvider_DataFromDbStorage.GetPrayerTimesAsync(
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
    public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> SemerkandDynamicPrayerTimeProvider_GetDataFromApi()
    {
        var result = _semerkandDynamicPrayerTimeProvider_DataFromApi.GetPrayerTimesAsync(
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
