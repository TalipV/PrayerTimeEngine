using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.DTOs;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Fazilet;

public class FaziletDynamicPrayerTimeProviderTests : BaseTest
{
    private readonly IFaziletRepository _faziletRepositoryMock;
    private readonly IFaziletApiService _faziletApiServiceMock;
    private readonly IPlaceService _placeServiceMock;
    private readonly FaziletDynamicPrayerTimeProvider _faziletDynamicPrayerTimeProvider;

    public FaziletDynamicPrayerTimeProviderTests()
    {
        _faziletRepositoryMock = Substitute.For<IFaziletRepository>();
        _faziletApiServiceMock = Substitute.For<IFaziletApiService>();
        _placeServiceMock = Substitute.For<IPlaceService>();

        _faziletDynamicPrayerTimeProvider =
            new FaziletDynamicPrayerTimeProvider(
                _faziletRepositoryMock,
                _faziletApiServiceMock,
                _placeServiceMock,
                Substitute.For<ILogger<FaziletDynamicPrayerTimeProvider>>());
    }

    #region GetPrayerTimesAsync

    [Fact]
    public async Task GetPrayerTimesAsync_AllValuesFromDbCache_Success()
    {
        // ARRANGE
        var date = new LocalDate(2024, 1, 1);
        ZonedDateTime dateInUtc = date.AtStartOfDayInZone(DateTimeZone.Utc);
        BaseLocationData locationData = new FaziletLocationData { CityName = "Berlin", CountryName = "Deutschland" };
        List<GenericSettingConfiguration> configurations =
            [
                new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, Source = EDynamicPrayerTimeProviderType.Fazilet }
            ];

        _faziletRepositoryMock.GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>()).Returns(1);
        _faziletRepositoryMock.GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>()).Returns(1);

        var times = new FaziletDailyPrayerTimes
        {
            CityID = 1,
            Date = date,
            TimeZone = DateTimeZone.Utc,
            Imsak = dateInUtc.PlusHours(4).ToInstant(),
            Fajr = dateInUtc.PlusHours(5).ToInstant(),
            Shuruq = dateInUtc.PlusHours(7).ToInstant(),
            Duha = dateInUtc.PlusHours(8).ToInstant(),
            Dhuhr = dateInUtc.PlusHours(12).ToInstant(),
            Asr = dateInUtc.PlusHours(15).ToInstant(),
            Maghrib = dateInUtc.PlusHours(18).ToInstant(),
            Isha = dateInUtc.PlusHours(20).ToInstant(),
        };

        _faziletRepositoryMock.GetTimesByDateAndCityID(
            Arg.Is<LocalDate>(x => x == date || x == date.PlusDays(1)),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(times);

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> calculationResult =
            await _faziletDynamicPrayerTimeProvider.GetPrayerTimesAsync(dateInUtc, locationData, configurations, default);

        // ASSERT
        calculationResult.Should().NotBeNull().And.HaveCount(1);
        calculationResult.First().Should().BeEquivalentTo((ETimeType.FajrEnd, times.GetZonedDateTimeForTimeType(ETimeType.FajrEnd).Value));

        _placeServiceMock.ReceivedCalls().Should().BeEmpty();
        _faziletApiServiceMock.ReceivedCalls().Should().BeEmpty();
        await _faziletRepositoryMock.Received(1).GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>());
        await _faziletRepositoryMock.Received(1).GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>());
        await _faziletRepositoryMock.Received(1).GetTimesByDateAndCityID(Arg.Is(date), Arg.Is(1), Arg.Any<CancellationToken>());
        await _faziletRepositoryMock.Received(1).GetTimesByDateAndCityID(Arg.Is(date.PlusDays(1)), Arg.Is(1), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPrayerTimesAsync_DayNotCached_RequestsApiForThatDayAndStoresDaysWithTimes()
    {
        // ARRANGE
        var date = new LocalDate(2026, 12, 24);
        ZonedDateTime dateInUtc = date.AtStartOfDayInZone(DateTimeZone.Utc);
        BaseLocationData locationData = new FaziletLocationData { CityName = "Berlin", CountryName = "Deutschland" };
        List<GenericSettingConfiguration> configurations =
            [
                new GenericSettingConfiguration { TimeType = ETimeType.FajrStart, Source = EDynamicPrayerTimeProviderType.Fazilet }
            ];

        _faziletRepositoryMock.GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>()).Returns(1);
        _faziletRepositoryMock.GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>()).Returns(1);

        // the API returns the requested day with the day before and after; the last one lies outside of its data range
        _faziletApiServiceMock.GetTimesByCityID(Arg.Is(1), Arg.Any<LocalDate>(), Arg.Any<CancellationToken>())
            .Returns(new FaziletGetTimesByCityIDResponseDTO
            {
                Timezone = DateTimeZone.Utc,
                PrayerTimes =
                [
                    CreateResponseDay(date.PlusDays(-1), withTimes: true),
                    CreateResponseDay(date, withTimes: true),
                    CreateResponseDay(date.PlusDays(1), withTimes: false),
                ]
            });

        List<FaziletDailyPrayerTimes>? insertedTimes = null;
        await _faziletRepositoryMock.InsertPrayerTimesAsync(
            Arg.Do<IEnumerable<FaziletDailyPrayerTimes>>(x => insertedTimes ??= [.. x]),
            Arg.Any<CancellationToken>());

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> calculationResult =
            await _faziletDynamicPrayerTimeProvider.GetPrayerTimesAsync(dateInUtc, locationData, configurations, default);

        // ASSERT
        calculationResult.Should().ContainSingle()
            .Which.Should().Be((ETimeType.FajrStart, dateInUtc.PlusHours(5)));

        // the day after (for the next Fajr) was not stored because it has no times, so it is requested on its own
        await _faziletApiServiceMock.Received(1).GetTimesByCityID(Arg.Is(1), Arg.Is(date), Arg.Any<CancellationToken>());
        await _faziletApiServiceMock.Received(1).GetTimesByCityID(Arg.Is(1), Arg.Is(date.PlusDays(1)), Arg.Any<CancellationToken>());
        insertedTimes.Should().NotBeNull();
        insertedTimes!.Select(x => x.Date).Should().BeEquivalentTo([date.PlusDays(-1), date]);
    }

    [Fact]
    public async Task GetPrayerTimesAsync_ApiHasNoTimesForDay_ThrowsNotAvailableException()
    {
        // ARRANGE
        var date = new LocalDate(2040, 6, 1);
        ZonedDateTime dateInUtc = date.AtStartOfDayInZone(DateTimeZone.Utc);
        BaseLocationData locationData = new FaziletLocationData { CityName = "Berlin", CountryName = "Deutschland" };
        List<GenericSettingConfiguration> configurations =
            [
                new GenericSettingConfiguration { TimeType = ETimeType.FajrStart, Source = EDynamicPrayerTimeProviderType.Fazilet }
            ];

        _faziletRepositoryMock.GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>()).Returns(1);
        _faziletRepositoryMock.GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>()).Returns(1);

        _faziletApiServiceMock.GetTimesByCityID(Arg.Is(1), Arg.Any<LocalDate>(), Arg.Any<CancellationToken>())
            .Returns(new FaziletGetTimesByCityIDResponseDTO
            {
                Timezone = DateTimeZone.Utc,
                PrayerTimes =
                [
                    CreateResponseDay(date.PlusDays(-1), withTimes: false),
                    CreateResponseDay(date, withTimes: false),
                    CreateResponseDay(date.PlusDays(1), withTimes: false),
                ]
            });

        // ACT
        Func<Task> action = () => _faziletDynamicPrayerTimeProvider.GetPrayerTimesAsync(dateInUtc, locationData, configurations, default);

        // ASSERT
        (await action.Should().ThrowAsync<PrayerTimesNotAvailableForDateException>())
            .Which.Date.Should().Be(date);
    }

    private static FaziletPrayerTimesResponseDTO CreateResponseDay(LocalDate date, bool withTimes)
    {
        List<TimeDetail> At(int hour) =>
            withTimes
                ? [new TimeDetail { OffsetDateTime = date.At(new LocalTime(hour, 0)).WithOffset(Offset.Zero) }]
                : [];

        return new FaziletPrayerTimesResponseDTO
        {
            Date = date,
            Imsak = At(4),
            Fajr = At(5),
            Shuruq = At(7),
            Duha = At(8),
            Dhuhr = At(12),
            Asr = At(15),
            Maghrib = At(18),
            Isha = At(20),
        };
    }

    #endregion GetPrayerTimesAsync

    #region GetLocationInfo

    [Fact]
    public async Task GetLocationInfo_CountryOnlyFoundByTurkishName_ReturnsLocationData()
    {
        // ARRANGE
        var completePlaceInfo = new ProfilePlaceInfo
        {
            ExternalID = "1",
            Longitude = 1M,
            Latitude = 1M,
            InfoLanguageCode = "de",
            Country = "Österreich",
            City = "Innsbruck",
            CityDistrict = "",
            PostCode = "6020",
            Street = "Straße",
            TimezoneInfo = new TimezoneInfo { Name = TestDataHelper.EUROPE_VIENNA_TIME_ZONE.Id },
        };

        var turkishBasicPlaceInfo =
            new BasicPlaceInfo
            {
                ExternalID = "1",
                Longitude = 1M,
                Latitude = 1M,
                InfoLanguageCode = "de",
                Country = "Avusturya",
                City = "Innsbruck",
                CityDistrict = "",
                PostCode = "6020",
                Street = "Yol",
            };

        _placeServiceMock
            .GetPlaceBasedOnPlace(Arg.Is(completePlaceInfo), Arg.Is("tr"), Arg.Any<CancellationToken>())
            .Returns(turkishBasicPlaceInfo);

        _faziletRepositoryMock.HasCountryData(Arg.Any<CancellationToken>()).Returns(true);
        _faziletRepositoryMock.GetCountryIDByName(Arg.Is("Avusturya"), Arg.Any<CancellationToken>()).Returns(1);
        _faziletRepositoryMock.GetCityIDByName(Arg.Is(1), Arg.Is("Innsbruck"), Arg.Any<CancellationToken>()).Returns(1);

        // ACT
        var locationData = await _faziletDynamicPrayerTimeProvider.GetLocationInfo(completePlaceInfo, default) as FaziletLocationData;

        // ASSERT
        locationData.Should().NotBeNull();
        locationData.CountryName.Should().Be("Avusturya");
        locationData.CityName.Should().Be("Innsbruck");
        locationData.Source.Should().Be(EDynamicPrayerTimeProviderType.Fazilet);
    }

    [Fact]
    public async Task GetLocationInfo_CityWithTurkishIOnlyKnownWithLatinI_StoresMatchedCityName()
    {
        // ARRANGE
        var completePlaceInfo = new ProfilePlaceInfo
        {
            ExternalID = "1",
            Longitude = 1M,
            Latitude = 1M,
            InfoLanguageCode = "tr",
            Country = "Türkiye",
            City = "İstanbul",
            CityDistrict = "",
            PostCode = "34000",
            Street = "Yol",
            TimezoneInfo = new TimezoneInfo { Name = "Europe/Istanbul" },
        };

        _faziletRepositoryMock.HasCountryData(Arg.Any<CancellationToken>()).Returns(true);
        _faziletRepositoryMock.GetCountryIDByName(Arg.Is("Türkiye"), Arg.Any<CancellationToken>()).Returns(1);

        // the city is only known with the latin 'I' version of its name
        _faziletRepositoryMock.HasCityData(Arg.Is(1), Arg.Any<CancellationToken>()).Returns(true);
        _faziletRepositoryMock.GetCityIDByName(Arg.Is(1), Arg.Is("Istanbul"), Arg.Any<CancellationToken>()).Returns(1);

        // ACT
        var locationData = await _faziletDynamicPrayerTimeProvider.GetLocationInfo(completePlaceInfo, default) as FaziletLocationData;

        // ASSERT
        locationData.Should().NotBeNull();
        locationData.CountryName.Should().Be("Türkiye");

        // the matched name version has to be stored because later look ups require exact matches
        locationData.CityName.Should().Be("Istanbul");
    }

    [Fact]
    public async Task GetLocationInfo_CountryFoundNativelyButCityOnlyByTurkishName_ReturnsLocationData()
    {
        // ARRANGE
        var completePlaceInfo = new ProfilePlaceInfo
        {
            ExternalID = "1",
            Longitude = 1M,
            Latitude = 1M,
            InfoLanguageCode = "de",
            Country = "Deutschland",
            City = "München",
            CityDistrict = "",
            PostCode = "80331",
            Street = "Straße",
            TimezoneInfo = new TimezoneInfo { Name = TestDataHelper.EUROPE_VIENNA_TIME_ZONE.Id },
        };

        var turkishBasicPlaceInfo =
            new BasicPlaceInfo
            {
                ExternalID = "1",
                Longitude = 1M,
                Latitude = 1M,
                InfoLanguageCode = "de",
                Country = "Almanya",
                City = "Münih",
                CityDistrict = "",
                PostCode = "80331",
                Street = "Yol",
            };

        _placeServiceMock
            .GetPlaceBasedOnPlace(Arg.Is(completePlaceInfo), Arg.Is("tr"), Arg.Any<CancellationToken>())
            .Returns(turkishBasicPlaceInfo);

        _faziletRepositoryMock.HasCountryData(Arg.Any<CancellationToken>()).Returns(true);
        _faziletRepositoryMock.HasCityData(Arg.Is(1), Arg.Any<CancellationToken>()).Returns(true);

        // the country is found through its native name, the city only through its turkish name
        _faziletRepositoryMock.GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>()).Returns(1);
        _faziletRepositoryMock.GetCityIDByName(Arg.Is(1), Arg.Is("Münih"), Arg.Any<CancellationToken>()).Returns(5);

        // ACT
        var locationData = await _faziletDynamicPrayerTimeProvider.GetLocationInfo(completePlaceInfo, default) as FaziletLocationData;

        // ASSERT
        locationData.Should().NotBeNull();
        locationData.CountryName.Should().Be("Deutschland");
        locationData.CityName.Should().Be("Münih");

        // the country resolution never needed the turkish names, only the city resolution did (with a single retrieval)
        await _placeServiceMock.Received(1).GetPlaceBasedOnPlace(Arg.Is(completePlaceInfo), Arg.Is("tr"), Arg.Any<CancellationToken>());
        await _faziletRepositoryMock.DidNotReceive().GetCountryIDByName(Arg.Is("Almanya"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetLocationInfo_MekkeAsCityName_StoresFaziletSpecificCityName()
    {
        // ARRANGE
        var completePlaceInfo = new ProfilePlaceInfo
        {
            ExternalID = "1",
            Longitude = 1M,
            Latitude = 1M,
            InfoLanguageCode = "tr",
            Country = "Suudi Arabistan",
            City = "Mekke",
            CityDistrict = "",
            PostCode = "",
            Street = "",
            TimezoneInfo = new TimezoneInfo { Name = "Asia/Riyadh" },
        };

        _faziletRepositoryMock.HasCountryData(Arg.Any<CancellationToken>()).Returns(true);
        _faziletRepositoryMock.GetCountryIDByName(Arg.Is("Suudi Arabistan"), Arg.Any<CancellationToken>()).Returns(1);

        // Fazilet only knows the city with its special name
        _faziletRepositoryMock.HasCityData(Arg.Is(1), Arg.Any<CancellationToken>()).Returns(true);
        _faziletRepositoryMock.GetCityIDByName(Arg.Is(1), Arg.Is("Mekke-i Mükerreme"), Arg.Any<CancellationToken>()).Returns(1);

        // ACT
        var locationData = await _faziletDynamicPrayerTimeProvider.GetLocationInfo(completePlaceInfo, default) as FaziletLocationData;

        // ASSERT
        locationData.Should().NotBeNull();
        locationData.CountryName.Should().Be("Suudi Arabistan");

        // the matched name version has to be stored because later look ups require exact matches
        locationData.CityName.Should().Be("Mekke-i Mükerreme");
    }

    #endregion GetLocationInfo
}
