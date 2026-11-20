using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.DTOs;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Services;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Waqti;

public class WaqtiDynamicPrayerTimeProviderTests : BaseTest
{
    private readonly IWaqtiRepository _waqtiRepositoryMock;
    private readonly IWaqtiApiService _waqtiApiServiceMock;
    private readonly WaqtiDynamicPrayerTimeProvider _waqtiDynamicPrayerTimeProvider;

    private static readonly WaqtiLocationData s_meccaLocationData = new()
    {
        Longitude = 39.8262M,
        Latitude = 21.4225M,
        TimezoneName = "Asia/Riyadh"
    };

    public WaqtiDynamicPrayerTimeProviderTests()
    {
        _waqtiRepositoryMock = Substitute.For<IWaqtiRepository>();
        _waqtiApiServiceMock = Substitute.For<IWaqtiApiService>();

        _waqtiDynamicPrayerTimeProvider =
            new WaqtiDynamicPrayerTimeProvider(
                _waqtiRepositoryMock,
                _waqtiApiServiceMock);
    }

    #region helpers

    /// <summary>
    /// A time whose raw instant (without safety minutes and rounding) lies 30 seconds after the
    /// published one, so that tests can tell them apart.
    /// </summary>
    private static WaqtiTimeResponseDTO Time(int year, int month, int day, int hour, int minute) =>
        new()
        {
            Instant = new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.Zero),
            RawInstant = new DateTimeOffset(year, month, day, hour, minute, 30, TimeSpan.Zero),
            Validity = "Valid"
        };

    /// <summary>
    /// A month response in which every time of a day is shifted by <paramref name="minuteOffset"/>
    /// so that responses of different methods can be told apart.
    /// </summary>
    private static List<WaqtiDailyPrayerTimesResponseDTO> CreateMonthResponse(int year, int month, string method, int minuteOffset = 0)
    {
        DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[s_meccaLocationData.TimezoneName];
        int dayCount = CalendarSystem.Iso.GetDaysInMonth(year, month);

        return [.. Enumerable.Range(1, dayCount).Select(day =>
        {
            LocalDate tomorrow = new LocalDate(year, month, day).PlusDays(1);

            return new WaqtiDailyPrayerTimesResponseDTO
            {
                Date = new LocalDate(year, month, day),
                Timezone = timeZone,
                Method = method,
                Fajr = Time(year, month, day, 2, 10 + minuteOffset),
                Shuruk = Time(year, month, day, 3, 30 + minuteOffset),
                Duha = Time(year, month, day, 3, 50 + minuteOffset),
                Dhuhr = Time(year, month, day, 9, 20 + minuteOffset),
                Asr = Time(year, month, day, 12, 40 + minuteOffset),
                AsrTwoShadowLength = Time(year, month, day, 13, 30 + minuteOffset),
                Maghrib = Time(year, month, day, 15, 10 + minuteOffset),
                Ishaa = Time(year, month, day, 16, 30 + minuteOffset),
                FajrTomorrow = Time(tomorrow.Year, tomorrow.Month, tomorrow.Day, 2, 11 + minuteOffset),
            };
        })];
    }

    private void SetupApiResponse(string method, int minuteOffset = 0)
    {
        _waqtiApiServiceMock
            .GetMonthPrayerTimesAsync(Arg.Is<WaqtiMonthPrayerTimesRequestDTO>(x => x.Method == method && x.Overrides == null), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                WaqtiMonthPrayerTimesRequestDTO request = callInfo.Arg<WaqtiMonthPrayerTimesRequestDTO>();
                return Task.FromResult(CreateMonthResponse(request.Year, request.Month, method, minuteOffset));
            });
    }

    /// <summary>
    /// Response of a request with angle overrides: Fajr and Isha are shifted by
    /// <paramref name="minuteOffset"/>, so that the times of different angle pairs can be told apart.
    /// </summary>
    private void SetupDerivedApiResponse(string method, double? fajrAngle, double? ishaaAngle, int minuteOffset)
    {
        _waqtiApiServiceMock
            .GetMonthPrayerTimesAsync(
                Arg.Is<WaqtiMonthPrayerTimesRequestDTO>(x =>
                    x.Method == method
                    && x.Overrides != null
                    && x.Overrides.FajrAngle == fajrAngle
                    && x.Overrides.IshaaAngle == ishaaAngle),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                WaqtiMonthPrayerTimesRequestDTO request = callInfo.Arg<WaqtiMonthPrayerTimesRequestDTO>();
                return Task.FromResult(CreateMonthResponse(request.Year, request.Month, method, minuteOffset));
            });
    }

    private static ZonedDateTime GetZonedDate(LocalDate date)
        => date.AtStartOfDayInZone(DateTimeZoneProviders.Tzdb[s_meccaLocationData.TimezoneName]);

    #endregion helpers

    [Fact]
    public async Task GetPrayerTimesAsync_AllSupportedTimeTypes_ReturnsCorrectTimes()
    {
        // ARRANGE
        var date = new LocalDate(2024, 2, 25);
        SetupApiResponse("Mwl2007");
        List<GenericSettingConfiguration> configurations =
        [
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrStart, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrEnd, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.DuhaStart, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrStart, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrEnd, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.AsrStart, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.AsrEnd, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.AsrMithlayn, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.MaghribStart, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.MaghribEnd, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.IshaStart, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.IshaEnd, Method = "Mwl2007" },
        ];

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
            await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(date), s_meccaLocationData, configurations, default);

        // ASSERT (Asia/Riyadh is UTC+3)
        result.Should().HaveCount(12);
        result.Single(x => x.TimeType == ETimeType.FajrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 5, 10));
        result.Single(x => x.TimeType == ETimeType.FajrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 6, 30));
        result.Single(x => x.TimeType == ETimeType.DuhaStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 6, 50));
        result.Single(x => x.TimeType == ETimeType.DhuhrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 12, 20));
        result.Single(x => x.TimeType == ETimeType.DhuhrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 15, 40));
        result.Single(x => x.TimeType == ETimeType.AsrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 15, 40));
        result.Single(x => x.TimeType == ETimeType.AsrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 18, 10));
        result.Single(x => x.TimeType == ETimeType.AsrMithlayn).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 16, 30));
        result.Single(x => x.TimeType == ETimeType.MaghribStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 18, 10));
        result.Single(x => x.TimeType == ETimeType.MaghribEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 19, 30));
        result.Single(x => x.TimeType == ETimeType.IshaStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 19, 30));
        result.Single(x => x.TimeType == ETimeType.IshaEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 26, 5, 11));
        result.Should().OnlyContain(x => x.ZonedDateTime.Zone.Id == "Asia/Riyadh");
    }

    [Fact]
    public async Task GetPrayerTimesAsync_NotCached_RequestsMonthOfTheDateWithLocationAndMethod()
    {
        // ARRANGE
        var date = new LocalDate(2024, 2, 25);
        SetupApiResponse("Diyanet");
        List<GenericSettingConfiguration> configurations =
        [
            new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrStart, Method = "Diyanet" },
        ];

        // ACT
        await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(date), s_meccaLocationData, configurations, default);

        // ASSERT
        await _waqtiApiServiceMock.Received(1).GetMonthPrayerTimesAsync(
            Arg.Is<WaqtiMonthPrayerTimesRequestDTO>(x =>
                x.Year == 2024
                && x.Month == 2
                && x.Latitude == s_meccaLocationData.Latitude
                && x.Longitude == s_meccaLocationData.Longitude
                && x.Timezone == "Asia/Riyadh"
                && x.Elevation == 0
                && x.Method == "Diyanet"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPrayerTimesAsync_NotCached_WholeMonthStoredWithRequestLocationAndMethod()
    {
        // ARRANGE
        var date = new LocalDate(2024, 2, 25);
        SetupApiResponse("Mwl2007");
        List<WaqtiDailyPrayerTimes>? insertedTimes = null;
        await _waqtiRepositoryMock.InsertPrayerTimesAsync(
            Arg.Do<IEnumerable<WaqtiDailyPrayerTimes>>(x => insertedTimes = [.. x]),
            Arg.Any<CancellationToken>());
        List<GenericSettingConfiguration> configurations =
        [
            new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrStart, Method = "Mwl2007" },
        ];

        // ACT
        await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(date), s_meccaLocationData, configurations, default);

        // ASSERT
        insertedTimes.Should().NotBeNull();
        insertedTimes.Should().HaveCount(29);   // February of a leap year
        insertedTimes.Select(x => x.Date).Should().OnlyHaveUniqueItems();
        insertedTimes.Should().OnlyContain(x =>
            x.Method == "Mwl2007"
            && x.Latitude == s_meccaLocationData.Latitude
            && x.Longitude == s_meccaLocationData.Longitude
            && x.Date.Year == 2024 && x.Date.Month == 2);
    }

    [Fact]
    public async Task GetPrayerTimesAsync_CachedInRepository_NoApiRequest()
    {
        // ARRANGE
        var date = new LocalDate(2024, 2, 25);
        WaqtiDailyPrayerTimes cachedTimes =
            CreateMonthResponse(2024, 2, "Mwl2007")
                .Single(x => x.Date == date)
                .ToWaqtiPrayerTimes(s_meccaLocationData.Longitude, s_meccaLocationData.Latitude);
        _waqtiRepositoryMock
            .GetPrayerTimesAsync(date, s_meccaLocationData.Longitude, s_meccaLocationData.Latitude, "Mwl2007", null, null, Arg.Any<CancellationToken>())
            .Returns(cachedTimes);
        List<GenericSettingConfiguration> configurations =
        [
            new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrStart, Method = "Mwl2007" },
        ];

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
            await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(date), s_meccaLocationData, configurations, default);

        // ASSERT
        result.Should().ContainSingle();
        result.Single().ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 12, 20));
        await _waqtiApiServiceMock.DidNotReceiveWithAnyArgs().GetMonthPrayerTimesAsync(default!, default);
        await _waqtiRepositoryMock.DidNotReceiveWithAnyArgs().InsertPrayerTimesAsync(default!, default);
    }

    [Fact]
    public async Task GetPrayerTimesAsync_TwoDifferentMethods_OneRequestPerMethodAndTimesFromTheirMethod()
    {
        // ARRANGE
        var date = new LocalDate(2024, 2, 25);
        SetupApiResponse("Mwl2007", minuteOffset: 0);
        SetupApiResponse("Diyanet", minuteOffset: 5);
        List<GenericSettingConfiguration> configurations =
        [
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrStart, Method = "Diyanet" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrEnd, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrStart, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.IshaEnd, Method = "Diyanet" },
        ];

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
            await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(date), s_meccaLocationData, configurations, default);

        // ASSERT
        result.Should().HaveCount(4);
        result.Single(x => x.TimeType == ETimeType.FajrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 5, 15));
        result.Single(x => x.TimeType == ETimeType.FajrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 6, 30));
        result.Single(x => x.TimeType == ETimeType.DhuhrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 12, 20));
        result.Single(x => x.TimeType == ETimeType.IshaEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 26, 5, 16));

        await _waqtiApiServiceMock.Received(1).GetMonthPrayerTimesAsync(Arg.Is<WaqtiMonthPrayerTimesRequestDTO>(x => x.Method == "Mwl2007"), Arg.Any<CancellationToken>());
        await _waqtiApiServiceMock.Received(1).GetMonthPrayerTimesAsync(Arg.Is<WaqtiMonthPrayerTimesRequestDTO>(x => x.Method == "Diyanet"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPrayerTimesAsync_DerivedTimeTypes_FajrAndIshaOfTheirAnglesWithRawTimes()
    {
        // ARRANGE
        var date = new LocalDate(2024, 2, 25);
        SetupApiResponse("Mwl2007");
        // Karaha -4.5° and Asr Karaha +4.5° share one request, Ghalas -7.5° and Ishtibak -10° another one
        SetupDerivedApiResponse("Mwl2007", fajrAngle: 4.5, ishaaAngle: -4.5, minuteOffset: 1);
        SetupDerivedApiResponse("Mwl2007", fajrAngle: 7.5, ishaaAngle: 10, minuteOffset: 2);
        List<GenericSettingConfiguration> configurations =
        [
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrStart, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Method = "Mwl2007", Degree = -7.5 },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Method = "Mwl2007", Degree = -4.5 },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.AsrKaraha, Method = "Mwl2007", Degree = 4.5 },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.MaghribIshtibak, Method = "Mwl2007", Degree = -10 },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.IshaStart, Method = "Mwl2007" },
        ];

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
            await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(date), s_meccaLocationData, configurations, default);

        // ASSERT
        result.Should().HaveCount(6);

        // regular times: published time (without the raw 30 seconds)
        result.Single(x => x.TimeType == ETimeType.FajrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 5, 10, 0));
        result.Single(x => x.TimeType == ETimeType.IshaStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 19, 30, 0));

        // derived times: Fajr / Isha of the request with their angle, raw time
        result.Single(x => x.TimeType == ETimeType.FajrKaraha).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 5, 11, 30));
        result.Single(x => x.TimeType == ETimeType.AsrKaraha).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 19, 31, 30));
        result.Single(x => x.TimeType == ETimeType.FajrGhalas).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 5, 12, 30));
        result.Single(x => x.TimeType == ETimeType.MaghribIshtibak).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 19, 32, 30));

        await _waqtiApiServiceMock.ReceivedWithAnyArgs(3).GetMonthPrayerTimesAsync(default!, default);
    }

    [Theory]
    [InlineData(ETimeType.FajrGhalas, -7.5, 7.5, null)]
    [InlineData(ETimeType.FajrKaraha, -4.5, 4.5, null)]
    [InlineData(ETimeType.AsrKaraha, 4.5, null, -4.5)]
    [InlineData(ETimeType.MaghribIshtibak, -10.0, null, 10.0)]
    public async Task GetPrayerTimesAsync_DerivedTimeType_AngleConvertedToWaqtiDepression(
        ETimeType timeType, double degree, double? expectedFajrAngle, double? expectedIshaaAngle)
    {
        // ARRANGE
        var date = new LocalDate(2024, 2, 25);
        SetupDerivedApiResponse("Mwl2007", expectedFajrAngle, expectedIshaaAngle, minuteOffset: 0);
        List<GenericSettingConfiguration> configurations =
        [
            new WaqtiCalculationConfiguration { TimeType = timeType, Method = "Mwl2007", Degree = degree },
        ];

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
            await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(date), s_meccaLocationData, configurations, default);

        // ASSERT
        result.Should().ContainSingle(x => x.TimeType == timeType);
        await _waqtiApiServiceMock.Received(1).GetMonthPrayerTimesAsync(
            Arg.Is<WaqtiMonthPrayerTimesRequestDTO>(x =>
                x.Overrides != null
                && x.Overrides.FajrAngle == expectedFajrAngle
                && x.Overrides.IshaaAngle == expectedIshaaAngle),
            Arg.Any<CancellationToken>());
        await _waqtiApiServiceMock.ReceivedWithAnyArgs(1).GetMonthPrayerTimesAsync(default!, default);
    }

    [Fact]
    public async Task GetPrayerTimesAsync_GhalasAndKarahaWithSameAngle_OneRequest()
    {
        // ARRANGE
        var date = new LocalDate(2024, 2, 25);
        SetupDerivedApiResponse("Mwl2007", fajrAngle: 6, ishaaAngle: null, minuteOffset: 0);
        List<GenericSettingConfiguration> configurations =
        [
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Method = "Mwl2007", Degree = -6 },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Method = "Mwl2007", Degree = -6 },
        ];

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
            await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(date), s_meccaLocationData, configurations, default);

        // ASSERT
        result.Should().HaveCount(2);
        await _waqtiApiServiceMock.ReceivedWithAnyArgs(1).GetMonthPrayerTimesAsync(default!, default);
    }

    [Fact]
    public async Task GetPrayerTimesAsync_TwoFajrAnglesWithoutIshaAngle_OneRequestPerFajrAngle()
    {
        // ARRANGE
        var date = new LocalDate(2024, 2, 25);
        SetupDerivedApiResponse("Mwl2007", fajrAngle: 4.5, ishaaAngle: null, minuteOffset: 1);
        SetupDerivedApiResponse("Mwl2007", fajrAngle: 7.5, ishaaAngle: null, minuteOffset: 2);
        List<GenericSettingConfiguration> configurations =
        [
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Method = "Mwl2007", Degree = -7.5 },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Method = "Mwl2007", Degree = -4.5 },
        ];

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
            await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(date), s_meccaLocationData, configurations, default);

        // ASSERT
        result.Single(x => x.TimeType == ETimeType.FajrKaraha).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 5, 11, 30));
        result.Single(x => x.TimeType == ETimeType.FajrGhalas).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 5, 12, 30));
        await _waqtiApiServiceMock.ReceivedWithAnyArgs(2).GetMonthPrayerTimesAsync(default!, default);
    }

    [Fact]
    public async Task GetPrayerTimesAsync_DerivedTimesCached_CacheLookedUpWithAngles()
    {
        // ARRANGE
        var date = new LocalDate(2024, 2, 25);
        WaqtiDailyPrayerTimes cachedTimes =
            CreateMonthResponse(2024, 2, "Mwl2007")
                .Single(x => x.Date == date)
                .ToWaqtiPrayerTimes(s_meccaLocationData.Longitude, s_meccaLocationData.Latitude, fajrAngle: 7.5, ishaaAngle: null);
        _waqtiRepositoryMock
            .GetPrayerTimesAsync(date, s_meccaLocationData.Longitude, s_meccaLocationData.Latitude, "Mwl2007", 7.5, null, Arg.Any<CancellationToken>())
            .Returns(cachedTimes);
        List<GenericSettingConfiguration> configurations =
        [
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Method = "Mwl2007", Degree = -7.5 },
        ];

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
            await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(date), s_meccaLocationData, configurations, default);

        // ASSERT
        result.Should().ContainSingle();
        result.Single().ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2024, 2, 25, 5, 10, 30));
        await _waqtiApiServiceMock.DidNotReceiveWithAnyArgs().GetMonthPrayerTimesAsync(default!, default);
    }

    [Fact]
    public async Task GetPrayerTimesAsync_DerivedTimesNotCached_StoredWithAngles()
    {
        // ARRANGE
        var date = new LocalDate(2024, 2, 25);
        SetupDerivedApiResponse("Mwl2007", fajrAngle: 7.5, ishaaAngle: 10, minuteOffset: 0);
        List<WaqtiDailyPrayerTimes>? insertedTimes = null;
        await _waqtiRepositoryMock.InsertPrayerTimesAsync(
            Arg.Do<IEnumerable<WaqtiDailyPrayerTimes>>(x => insertedTimes = [.. x]),
            Arg.Any<CancellationToken>());
        List<GenericSettingConfiguration> configurations =
        [
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Method = "Mwl2007", Degree = -7.5 },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.MaghribIshtibak, Method = "Mwl2007", Degree = -10 },
        ];

        // ACT
        await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(date), s_meccaLocationData, configurations, default);

        // ASSERT
        insertedTimes.Should().HaveCount(29);
        insertedTimes.Should().OnlyContain(x => x.FajrAngle == 7.5 && x.IshaaAngle == 10 && x.Method == "Mwl2007");
    }

    [Theory]
    [InlineData(ETimeType.FajrGhalas)]
    [InlineData(ETimeType.FajrKaraha)]
    [InlineData(ETimeType.AsrKaraha)]
    [InlineData(ETimeType.MaghribIshtibak)]
    public async Task GetPrayerTimesAsync_DerivedTimeTypeWithoutDegree_ThrowsArgumentException(ETimeType timeType)
    {
        // ARRANGE
        List<GenericSettingConfiguration> configurations =
        [
            new WaqtiCalculationConfiguration { TimeType = timeType, Method = "Mwl2007" },
        ];

        // ACT
        Func<Task> action = () => _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(new LocalDate(2024, 2, 25)), s_meccaLocationData, configurations, default);

        // ASSERT
        await action.Should().ThrowAsync<ArgumentException>();
        await _waqtiApiServiceMock.DidNotReceiveWithAnyArgs().GetMonthPrayerTimesAsync(default!, default);
    }

    [Fact]
    public async Task GetPrayerTimesAsync_GenericConfigurationWithoutMethod_DefaultMethodUsed()
    {
        // ARRANGE
        var date = new LocalDate(2024, 2, 25);
        SetupApiResponse(WaqtiMethods.DEFAULT_METHOD);
        List<GenericSettingConfiguration> configurations =
        [
            new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = EDynamicPrayerTimeProviderType.Waqti },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.AsrStart, Method = "" },
        ];

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
            await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(date), s_meccaLocationData, configurations, default);

        // ASSERT
        result.Should().HaveCount(2);
        await _waqtiApiServiceMock.Received(1).GetMonthPrayerTimesAsync(
            Arg.Is<WaqtiMonthPrayerTimesRequestDTO>(x => x.Method == WaqtiMethods.DEFAULT_METHOD),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPrayerTimesAsync_ApiOmitsSomeTimes_MissingTimesNotReturnedWithoutError()
    {
        // ARRANGE
        var date = new LocalDate(2024, 2, 25);
        List<WaqtiDailyPrayerTimesResponseDTO> response = CreateMonthResponse(2024, 2, "Mwl2007");
        WaqtiDailyPrayerTimesResponseDTO day = response.Single(x => x.Date == date);
        day.Fajr = null;
        day.Ishaa = new WaqtiTimeResponseDTO { Instant = null, Validity = "Invalid" };
        _waqtiApiServiceMock
            .GetMonthPrayerTimesAsync(Arg.Any<WaqtiMonthPrayerTimesRequestDTO>(), Arg.Any<CancellationToken>())
            .Returns(response);
        List<GenericSettingConfiguration> configurations =
        [
            new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrStart, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrStart, Method = "Mwl2007" },
            new WaqtiCalculationConfiguration { TimeType = ETimeType.IshaStart, Method = "Mwl2007" },
        ];

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
            await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(date), s_meccaLocationData, configurations, default);

        // ASSERT
        result.Should().NotBeNull();

        // the available time is returned, the missing ones are simply absent
        result.Should().HaveCount(1);
        result.Single().TimeType.Should().Be(ETimeType.DhuhrStart);
    }

    [Fact]
    public async Task GetPrayerTimesAsync_ApiResponseWithoutRequestedDay_NothingReturnedWithoutError()
    {
        // ARRANGE
        var date = new LocalDate(2024, 2, 25);
        _waqtiApiServiceMock
            .GetMonthPrayerTimesAsync(Arg.Any<WaqtiMonthPrayerTimesRequestDTO>(), Arg.Any<CancellationToken>())
            .Returns([.. CreateMonthResponse(2024, 2, "Mwl2007").Where(x => x.Date != date)]);
        List<GenericSettingConfiguration> configurations =
        [
            new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrStart, Method = "Mwl2007" },
        ];

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
            await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(date), s_meccaLocationData, configurations, default);

        // ASSERT
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPrayerTimesAsync_NoConfigurations_NoRequestAndEmptyResult()
    {
        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
            await _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(GetZonedDate(new LocalDate(2024, 2, 25)), s_meccaLocationData, [], default);

        // ASSERT
        result.Should().BeEmpty();
        await _waqtiApiServiceMock.DidNotReceiveWithAnyArgs().GetMonthPrayerTimesAsync(default!, default);
    }

    [Fact]
    public async Task GetPrayerTimesAsync_LocationDataOfOtherProvider_ThrowsException()
    {
        // ARRANGE
        var muwaqqitLocationData = new MuwaqqitLocationData
        {
            Longitude = 39.8262M,
            Latitude = 21.4225M,
            TimezoneName = "Asia/Riyadh"
        };

        // ACT
        Func<Task> action = () => _waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(
            GetZonedDate(new LocalDate(2024, 2, 25)),
            muwaqqitLocationData,
            [new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrStart, Method = "Mwl2007" }],
            default);

        // ASSERT
        await action.Should().ThrowAsync<Exception>().WithMessage("*Waqti*");
    }

    [Fact]
    public async Task GetLocationInfo_CompletePlaceInfo_CoordinatesAndTimezoneTaken()
    {
        // ARRANGE
        var placeInfo = new ProfilePlaceInfo
        {
            ExternalID = "1",
            Longitude = 11.4041024M,
            Latitude = 47.2692124M,
            InfoLanguageCode = "de",
            Country = "Österreich",
            City = "Innsbruck",
            CityDistrict = "",
            PostCode = "6020",
            Street = "",
            TimezoneInfo = new TimezoneInfo
            {
                DisplayName = "CEST",
                Name = "Europe/Vienna",
                UtcOffsetSeconds = 7200
            }
        };

        // ACT
        BaseLocationData locationData = await _waqtiDynamicPrayerTimeProvider.GetLocationInfo(placeInfo, default);

        // ASSERT
        locationData.Should().BeOfType<WaqtiLocationData>();
        locationData.Source.Should().Be(EDynamicPrayerTimeProviderType.Waqti);
        locationData.Should().BeEquivalentTo(new WaqtiLocationData
        {
            Longitude = 11.4041024M,
            Latitude = 47.2692124M,
            TimezoneName = "Europe/Vienna"
        });
    }

    [Fact]
    public async Task GetLocationInfo_NullPlace_ThrowsArgumentNullException()
    {
        // ACT
        Func<Task> action = () => _waqtiDynamicPrayerTimeProvider.GetLocationInfo(null!, default);

        // ASSERT
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void GetUnsupportedTimeTypes_Called_ReturnsEmptySet()
    {
        // ACT
        HashSet<ETimeType> result = _waqtiDynamicPrayerTimeProvider.GetUnsupportedTimeTypes();

        // ASSERT
        result.Should().BeEmpty();
    }
}
