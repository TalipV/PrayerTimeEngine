﻿using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Fazilet;

public class FaziletDynamicPrayerTimeProviderTests : BaseTest
{
    private readonly IFaziletDBAccess _faziletDBAccessMock;
    private readonly IFaziletApiService _faziletApiServiceMock;
    private readonly IPlaceService _placeServiceMock;
    private readonly FaziletDynamicPrayerTimeProvider _faziletDynamicPrayerTimeProvider;

    public FaziletDynamicPrayerTimeProviderTests()
    {
        _faziletDBAccessMock = Substitute.For<IFaziletDBAccess>();
        _faziletApiServiceMock = Substitute.For<IFaziletApiService>();
        _placeServiceMock = Substitute.For<IPlaceService>();

        _faziletDynamicPrayerTimeProvider =
            new FaziletDynamicPrayerTimeProvider(
                _faziletDBAccessMock,
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

        _faziletDBAccessMock.GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>()).Returns(1);
        _faziletDBAccessMock.GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>()).Returns(1);

        var times = new FaziletDailyPrayerTimes
        {
            CityID = 1,
            Date = dateInUtc,
            Imsak = dateInUtc.PlusHours(4),
            Fajr = dateInUtc.PlusHours(5),
            Shuruq = dateInUtc.PlusHours(7),
            Duha = dateInUtc.PlusHours(8),
            Dhuhr = dateInUtc.PlusHours(12),
            Asr = dateInUtc.PlusHours(15),
            Maghrib = dateInUtc.PlusHours(18),
            Isha = dateInUtc.PlusHours(20),
        };

        _faziletDBAccessMock.GetTimesByDateAndCityID(
            Arg.Is<ZonedDateTime>(x => x == dateInUtc || x == dateInUtc.Plus(Duration.FromDays(1))),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(times);

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> calculationResult =
            await _faziletDynamicPrayerTimeProvider.GetPrayerTimesAsync(dateInUtc, locationData, configurations, default);

        // ASSERT
        calculationResult.Should().NotBeNull().And.HaveCount(1);
        calculationResult.First().Should().BeEquivalentTo((ETimeType.FajrEnd, times.Shuruq));

        _placeServiceMock.ReceivedCalls().Should().BeEmpty();
        _faziletApiServiceMock.ReceivedCalls().Should().BeEmpty();
        _faziletDBAccessMock.ReceivedCalls().Should().HaveCount(4);
        await _faziletDBAccessMock.Received(1).GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>());
        await _faziletDBAccessMock.Received(1).GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>());
        await _faziletDBAccessMock.Received(1).GetTimesByDateAndCityID(Arg.Is(dateInUtc), Arg.Is(1), Arg.Any<CancellationToken>());
        await _faziletDBAccessMock.Received(1).GetTimesByDateAndCityID(Arg.Is(dateInUtc.Plus(Duration.FromDays(1))), Arg.Is(1), Arg.Any<CancellationToken>());
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
        _faziletDBAccessMock.GetCountryIDByName(Arg.Is("Avusturya"), Arg.Any<CancellationToken>()).Returns(1);
        _faziletDBAccessMock.GetCityIDByName(Arg.Is(1), Arg.Is("Innsbruck"), Arg.Any<CancellationToken>()).Returns(1);

        // ACT
        var locationData = await _faziletDynamicPrayerTimeProvider.GetLocationInfo(completePlaceInfo, default) as FaziletLocationData;

        // ASSERT
        locationData.Should().NotBeNull();
        locationData.CountryName.Should().Be("Avusturya");
        locationData.CityName.Should().Be("Innsbruck");
        locationData.Source.Should().Be(EDynamicPrayerTimeProviderType.Fazilet);
    }

    #endregion GetLocationInfo
}
