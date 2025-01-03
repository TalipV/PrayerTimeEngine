using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NSubstitute.ReturnsExtensions;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.DTOs;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Semerkand;

public class SemerkandDynamicPrayerTimeProviderTests : BaseTest
{
    private readonly ISemerkandDBAccess _semerkandDBAccessMock;
    private readonly ISemerkandApiService _semerkandApiServiceMock;
    private readonly IPlaceService _placeServiceMock;
    private readonly SemerkandDynamicPrayerTimeProvider _semerkandDynamicPrayerTimeProvider;

    public SemerkandDynamicPrayerTimeProviderTests()
    {
        _semerkandDBAccessMock = Substitute.For<ISemerkandDBAccess>();
        _semerkandApiServiceMock = Substitute.For<ISemerkandApiService>();
        _placeServiceMock = Substitute.For<IPlaceService>();

        _semerkandDynamicPrayerTimeProvider =
            new SemerkandDynamicPrayerTimeProvider(
                _semerkandDBAccessMock,
                _semerkandApiServiceMock,
                _placeServiceMock,
                Substitute.For<ILogger<SemerkandDynamicPrayerTimeProvider>>());
    }

    #region GetPrayerTimesAsync

    [Fact]
    public async Task GetPrayerTimesAsync_AllValuesFromDbCache_Success()
    {
        // ARRANGE
        var date = new LocalDate(2024, 1, 1);
        ZonedDateTime zonedDate = date.AtStartOfDayInZone(TestDataHelper.EUROPE_BERLIN_TIME_ZONE);
        BaseLocationData locationData = new SemerkandLocationData { CityName = "Berlin", CountryName = "Deutschland", TimezoneName = TestDataHelper.EUROPE_BERLIN_TIME_ZONE.Id };
        List<GenericSettingConfiguration> configurations =
            [
                new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, Source = EDynamicPrayerTimeProviderType.Semerkand }
            ];

        _semerkandDBAccessMock.GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>()).Returns(1);
        _semerkandDBAccessMock.GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>()).Returns(1);

        var times = new SemerkandDailyPrayerTimes
        {
            CityID = 1,
            DayOfYear = 5,
            Date = zonedDate,
            Fajr = zonedDate.PlusHours(5),
            Shuruq = zonedDate.PlusHours(7),
            Dhuhr = zonedDate.PlusHours(12),
            Asr = zonedDate.PlusHours(15),
            Maghrib = zonedDate.PlusHours(18),
            Isha = zonedDate.PlusHours(20),
        };

        _semerkandDBAccessMock.GetTimesByDateAndCityID(
                Arg.Is<ZonedDateTime>(x => x == zonedDate || x == zonedDate.Plus(Duration.FromDays(1))),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(times);

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> calculationResult =
            await _semerkandDynamicPrayerTimeProvider.GetPrayerTimesAsync(zonedDate, locationData, configurations, default);

        // ASSERT
        calculationResult.Should().NotBeNull().And.HaveCount(1);
        calculationResult.First().Should().BeEquivalentTo((ETimeType.FajrEnd, times.Shuruq));

        _placeServiceMock.ReceivedCalls().Should().BeEmpty();
        _semerkandApiServiceMock.ReceivedCalls().Should().BeEmpty();
        _semerkandDBAccessMock.ReceivedCalls().Should().HaveCount(4);
        await _semerkandDBAccessMock.Received(1).GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>());
        await _semerkandDBAccessMock.Received(1).GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>());
        await _semerkandDBAccessMock.Received(1).GetTimesByDateAndCityID(Arg.Is(zonedDate), Arg.Is(1), Arg.Any<CancellationToken>());
        await _semerkandDBAccessMock.Received(1).GetTimesByDateAndCityID(Arg.Is(zonedDate.Plus(Duration.FromDays(1))), Arg.Is(1), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPrayerTimesAsync_AllValuesFromApi_Success()
    {
        // ARRANGE
        DateTimeZone dateTimeZone = TestDataHelper.EUROPE_BERLIN_TIME_ZONE;
        var date = new LocalDate(2024, 12, 31);
        var nextFajrDate = new LocalDate(2025, 1, 1);

        ZonedDateTime zonedDateTime = date.AtStartOfDayInZone(dateTimeZone);
        ZonedDateTime nextFajrZonedDateTime = nextFajrDate.AtStartOfDayInZone(dateTimeZone);

        var shuruqZonedDateTime1 = zonedDateTime.PlusHours(7);
        var semerkandPrayerTimesDTO1 = new SemerkandPrayerTimesResponseDTO
        {
            DayOfYear = zonedDateTime.DayOfYear,
            Fajr = zonedDateTime.PlusHours(5).TimeOfDay,
            Shuruq = shuruqZonedDateTime1.TimeOfDay,
            Dhuhr = zonedDateTime.PlusHours(12).TimeOfDay,
            Asr = zonedDateTime.PlusHours(15).TimeOfDay,
            Maghrib = zonedDateTime.PlusHours(18).TimeOfDay,
            Isha = zonedDateTime.PlusHours(20).TimeOfDay,
        };
        _semerkandApiServiceMock.GetTimesByCityID(
                year: Arg.Is(date.Year),
                cityID: Arg.Is(1),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns([semerkandPrayerTimesDTO1]);

        var semerkandPrayerTimesDTO2 = new SemerkandPrayerTimesResponseDTO
        {
            DayOfYear = 1,
            Fajr = nextFajrZonedDateTime.PlusHours(5).TimeOfDay,
            Shuruq = nextFajrZonedDateTime.PlusHours(7).TimeOfDay,
            Dhuhr = nextFajrZonedDateTime.PlusHours(12).TimeOfDay,
            Asr = nextFajrZonedDateTime.PlusHours(15).TimeOfDay,
            Maghrib = nextFajrZonedDateTime.PlusHours(18).TimeOfDay,
            Isha = nextFajrZonedDateTime.PlusHours(20).TimeOfDay,
        };

        _semerkandApiServiceMock.GetTimesByCityID(
                year: Arg.Is(nextFajrDate.Year),
                cityID: Arg.Is(1),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns([semerkandPrayerTimesDTO2]);

        _semerkandDBAccessMock.GetCountryIDByName(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsNull();
        _semerkandDBAccessMock.GetCityIDByName(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsNull();
        _semerkandDBAccessMock.HasCountryData(Arg.Any<CancellationToken>()).Returns(false);

        _semerkandApiServiceMock.GetCountries(Arg.Any<CancellationToken>()).Returns([new SemerkandCountryResponseDTO { Name = "Deutschland", ID = 1 }]);
        _semerkandApiServiceMock.GetCitiesByCountryID(Arg.Is(1), Arg.Any<CancellationToken>()).Returns([new SemerkandCityResponseDTO { Name = "Berlin", ID = 1 }]);

        var locationData = new SemerkandLocationData { CityName = "Berlin", CountryName = "Deutschland", TimezoneName = dateTimeZone.Id };
        List<GenericSettingConfiguration> configurations =
            [
                new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, Source = EDynamicPrayerTimeProviderType.Semerkand }
            ];

        // ACT
        var calculationResult = await _semerkandDynamicPrayerTimeProvider.GetPrayerTimesAsync(zonedDateTime, locationData, configurations, default);

        // ASSERT
        calculationResult.Should().NotBeNull().And.HaveCount(1);
        calculationResult.Should().HaveCount(1);
        calculationResult.First().Should().BeEquivalentTo((ETimeType.FajrEnd, shuruqZonedDateTime1));

        _semerkandApiServiceMock.ReceivedCalls().Should().HaveCount(4);
        _semerkandDBAccessMock.ReceivedCalls().Should().HaveCount(10);
        await _semerkandApiServiceMock.Received(1).GetCountries(Arg.Any<CancellationToken>());
        await _semerkandApiServiceMock.Received(1).GetCitiesByCountryID(Arg.Is(1), Arg.Any<CancellationToken>());
        await _semerkandApiServiceMock.Received(1).GetTimesByCityID(Arg.Is(date.Year), Arg.Is(1), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPrayerTimesAsync_OnlyTimesFromApi_Success()
    {
        // ARRANGE
        DateTimeZone dateTimeZone = TestDataHelper.EUROPE_BERLIN_TIME_ZONE;
        var date = new LocalDate(2024, 12, 31);
        var nextFajrDate = new LocalDate(2025, 1, 1);

        ZonedDateTime zonedDateTime = date.AtStartOfDayInZone(dateTimeZone);
        ZonedDateTime nextFajrZonedDateTime = nextFajrDate.AtStartOfDayInZone(dateTimeZone);

        var shuruqZonedDateTime1 = zonedDateTime.PlusHours(7);
        var semerkandPrayerTimesDTO1 = new SemerkandPrayerTimesResponseDTO
        {
            DayOfYear = zonedDateTime.DayOfYear,
            Fajr = zonedDateTime.PlusHours(5).TimeOfDay,
            Shuruq = shuruqZonedDateTime1.TimeOfDay,
            Dhuhr = zonedDateTime.PlusHours(12).TimeOfDay,
            Asr = zonedDateTime.PlusHours(15).TimeOfDay,
            Maghrib = zonedDateTime.PlusHours(18).TimeOfDay,
            Isha = zonedDateTime.PlusHours(20).TimeOfDay,
        };
        _semerkandApiServiceMock.GetTimesByCityID(
                year: Arg.Is(date.Year),
                cityID: Arg.Is(1),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns([semerkandPrayerTimesDTO1]);

        var semerkandPrayerTimesDTO2 = new SemerkandPrayerTimesResponseDTO
        {
            DayOfYear = 1,
            Fajr = nextFajrZonedDateTime.PlusHours(5).TimeOfDay,
            Shuruq = nextFajrZonedDateTime.PlusHours(7).TimeOfDay,
            Dhuhr = nextFajrZonedDateTime.PlusHours(12).TimeOfDay,
            Asr = nextFajrZonedDateTime.PlusHours(15).TimeOfDay,
            Maghrib = nextFajrZonedDateTime.PlusHours(18).TimeOfDay,
            Isha = nextFajrZonedDateTime.PlusHours(20).TimeOfDay,
        };

        _semerkandApiServiceMock.GetTimesByCityID(
                year: Arg.Is(nextFajrDate.Year),
                cityID: Arg.Is(1),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns([semerkandPrayerTimesDTO2]);

        _semerkandDBAccessMock.GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>()).Returns(1);
        _semerkandDBAccessMock.GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>()).Returns(1);
        _semerkandDBAccessMock.HasCountryData(Arg.Any<CancellationToken>()).Returns(true);

        var locationData = new SemerkandLocationData { CityName = "Berlin", CountryName = "Deutschland", TimezoneName = dateTimeZone.Id };
        List<GenericSettingConfiguration> configurations =
            [
                new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, Source = EDynamicPrayerTimeProviderType.Semerkand }
            ];

        // ACT
        var calculationResult = await _semerkandDynamicPrayerTimeProvider.GetPrayerTimesAsync(zonedDateTime, locationData, configurations, default);

        // ASSERT
        calculationResult.Should().NotBeNull().And.HaveCount(1);
        calculationResult.Should().HaveCount(1);
        calculationResult.First().Should().BeEquivalentTo((ETimeType.FajrEnd, shuruqZonedDateTime1));

        _semerkandDBAccessMock.ReceivedCalls().Should().HaveCount(6);
        await _semerkandDBAccessMock.Received(1).GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>());
        await _semerkandDBAccessMock.Received(1).GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>());

        await _semerkandDBAccessMock.Received(1).GetTimesByDateAndCityID(Arg.Is(zonedDateTime), Arg.Is(1), Arg.Any<CancellationToken>());
        await _semerkandDBAccessMock.Received(1).InsertPrayerTimesAsync(Arg.Is<List<SemerkandDailyPrayerTimes>>(times => times[0].Date == zonedDateTime), Arg.Any<CancellationToken>());

        await _semerkandDBAccessMock.Received(1).GetTimesByDateAndCityID(Arg.Is(nextFajrZonedDateTime), Arg.Is(1), Arg.Any<CancellationToken>());
        await _semerkandDBAccessMock.Received(1).InsertPrayerTimesAsync(Arg.Is<List<SemerkandDailyPrayerTimes>>(times => times[0].Date == nextFajrZonedDateTime), Arg.Any<CancellationToken>());

        _semerkandApiServiceMock.ReceivedCalls().Should().HaveCount(2);
        await _semerkandApiServiceMock.Received(0).GetCountries(Arg.Any<CancellationToken>());
        await _semerkandApiServiceMock.Received(0).GetCitiesByCountryID(Arg.Is(1), Arg.Any<CancellationToken>());
        await _semerkandApiServiceMock.Received(1).GetTimesByCityID(Arg.Is(date.Year), Arg.Is(1), Arg.Any<CancellationToken>());
        await _semerkandApiServiceMock.Received(1).GetTimesByCityID(Arg.Is(nextFajrDate.Year), Arg.Is(1), Arg.Any<CancellationToken>());
    }

    #endregion GetPrayerTimesAsync

    #region GetLocationInfo

    [Fact]
    public async Task GetLocationInfo_X_X()
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
        _semerkandDBAccessMock.GetCountryIDByName(Arg.Is("Avusturya"), Arg.Any<CancellationToken>()).Returns(1);
        _semerkandDBAccessMock.GetCityIDByName(Arg.Is(1), Arg.Is("Innsbruck"), Arg.Any<CancellationToken>()).Returns(1);

        // ACT
        var locationData = await _semerkandDynamicPrayerTimeProvider.GetLocationInfo(completePlaceInfo, default) as SemerkandLocationData;

        // ASSERT
        locationData.Should().NotBeNull();
        locationData.CountryName.Should().Be("Avusturya");
        locationData.CityName.Should().Be("Innsbruck");
        locationData.Source.Should().Be(EDynamicPrayerTimeProviderType.Semerkand);
        locationData.TimezoneName.Should().Be(TestDataHelper.EUROPE_VIENNA_TIME_ZONE.Id);
    }

    #endregion GetLocationInfo
}
