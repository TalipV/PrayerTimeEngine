using NodaTime;
using NodaTime.TimeZones;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.DTOs;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Semerkand;

public class SemerkandPrayerTimesResponseDTOTests : BaseTest
{
    // Europe/Berlin DST transitions in 2024:
    //   Spring forward: 2024-03-31 02:00 CET → 03:00 CEST  (02:00-03:00 local does not exist)
    //   Fall back:      2024-10-27 03:00 CEST → 02:00 CET  (02:00-03:00 local occurs twice)
    private static readonly DateTimeZone _berlin = TestDataHelper.EUROPE_BERLIN_TIME_ZONE;

    private static SemerkandDailyPrayerTimes convert(SemerkandPrayerTimesResponseDTO dto, SemerkandDailyPrayerTimes previousDay = null)
        => dto.ToSemerkandPrayerTimes(
            cityID: 1,
            dateTimeZone: _berlin,
            firstDayOfYear: new LocalDate(2024, 1, 1),
            previousDayPrayerTimes: previousDay);

    private static SemerkandPrayerTimesResponseDTO dtoWithFajr(LocalDate date, LocalTime? fajr)
        => new() { DayOfYear = date.DayOfYear, Fajr = fajr };

    private static SemerkandDailyPrayerTimes previousDayWithFajr(LocalDate date, Instant? fajr)
        => new()
        {
            CityID = 1,
            Date = date, TimeZone = _berlin,
            Fajr = fajr, 
            Shuruq = null, Dhuhr = null, Asr = null, Maghrib = null, Isha = null,
        };

    [Fact]
    public void ToSemerkandPrayerTimes_UnambiguousTime_ReturnsItsSingleInstant()
    {
        // ARRANGE
        // 06:00 local on the fall-back day lies after the transition, so CET (UTC+1) applies
        SemerkandPrayerTimesResponseDTO dto = dtoWithFajr(new LocalDate(2024, 10, 27), fajr: new LocalTime(6, 0));

        // ACT
        SemerkandDailyPrayerTimes result = convert(dto, previousDay: null);

        // ASSERT
        result.Fajr.Should().Be(Instant.FromUtc(2024, 10, 27, 5, 0, 0));
    }

    [Fact]
    public void ToSemerkandPrayerTimes_SkippedTime_ThrowsSkippedTimeException()
    {
        // ARRANGE
        // on 2024-03-31 the clock jumps from 02:00 to 03:00, so 02:30 local does not exist
        var springForwardDay = new LocalDate(2024, 3, 31);
        SemerkandPrayerTimesResponseDTO dto = dtoWithFajr(springForwardDay, fajr: new LocalTime(2, 30));

        // ACT
        Func<SemerkandDailyPrayerTimes> func = () => convert(dto, previousDay: null);

        // ASSERT
        func.Should().Throw<SkippedTimeException>();
    }

    [Fact]
    public void ToSemerkandPrayerTimes_AmbiguousTimeWithoutPreviousDay_ThrowsAmbiguousTimeException()
    {
        // ARRANGE
        SemerkandPrayerTimesResponseDTO dto = dtoWithFajr(new LocalDate(2024, 10, 27), fajr: new LocalTime(2, 30));

        // ACT
        Func<SemerkandDailyPrayerTimes> func = () => convert(dto, previousDay: null);

        // ASSERT
        func.Should().Throw<AmbiguousTimeException>();
    }

    [Fact]
    public void ToSemerkandPrayerTimes_AmbiguousTimeWithPreviousDayMissingThatTime_ThrowsAmbiguousTimeException()
    {
        // ARRANGE
        SemerkandDailyPrayerTimes previousDay = previousDayWithFajr(new LocalDate(2024, 10, 26), fajr: null);
        SemerkandPrayerTimesResponseDTO dto = dtoWithFajr(new LocalDate(2024, 10, 27), fajr: new LocalTime(2, 30));

        // ACT
        Func<SemerkandDailyPrayerTimes> func = () => convert(dto, previousDay);

        // ASSERT
        func.Should().Throw<AmbiguousTimeException>();
    }

    [Fact]
    public void ToSemerkandPrayerTimes_AmbiguousTimeBeforeTransition_ReturnsEarlierInstant()
    {
        // ARRANGE
        // the previous day's time was 02:28 CEST (2024-10-26 00:28 UTC), so today's true time
        // is expected around 00:28 UTC + 24h, which is close to the earlier option (00:30 UTC)
        // and far from the later option (01:30 UTC)
        SemerkandDailyPrayerTimes previousDay =
            previousDayWithFajr(new LocalDate(2024, 10, 26), fajr: Instant.FromUtc(2024, 10, 26, 0, 28, 0));
        SemerkandPrayerTimesResponseDTO dto = dtoWithFajr(new LocalDate(2024, 10, 27), fajr: new LocalTime(2, 30));

        // ACT
        SemerkandDailyPrayerTimes result = convert(dto, previousDay);

        // ASSERT
        result.Fajr.Should().Be(Instant.FromUtc(2024, 10, 27, 0, 30, 0));
    }

    [Fact]
    public void ToSemerkandPrayerTimes_AmbiguousTimeAfterTransition_ReturnsLaterInstant()
    {
        // ARRANGE
        // the previous day's time was 03:28 CEST (2024-10-26 01:28 UTC): on the fall-back day
        // the same solar event lands on 02:30 local again because the clock was set back one hour.
        // Today's true time is expected around 01:28 UTC + 24h, which is close to the later
        // option (01:30 UTC) and far from the earlier option (00:30 UTC)
        SemerkandDailyPrayerTimes previousDay =
            previousDayWithFajr(new LocalDate(2024, 10, 26), fajr: Instant.FromUtc(2024, 10, 26, 1, 28, 0));
        SemerkandPrayerTimesResponseDTO dto = dtoWithFajr(new LocalDate(2024, 10, 27), fajr: new LocalTime(2, 30));

        // ACT
        SemerkandDailyPrayerTimes result = convert(dto, previousDay);

        // ASSERT
        result.Fajr.Should().Be(Instant.FromUtc(2024, 10, 27, 1, 30, 0));
    }

    [Fact]
    public void ToSemerkandPrayerTimes_AmbiguousTimeWithNonAdjacentPreviousDay_ThrowsArgumentException()
    {
        // ARRANGE
        // the "previous day" lies three days back, which is a usage error
        SemerkandDailyPrayerTimes previousDay =
            previousDayWithFajr(new LocalDate(2024, 10, 24), fajr: Instant.FromUtc(2024, 10, 24, 1, 28, 0));
        SemerkandPrayerTimesResponseDTO dto = dtoWithFajr(new LocalDate(2024, 10, 27), fajr: new LocalTime(2, 30));

        // ACT
        Func<SemerkandDailyPrayerTimes> func = () => convert(dto, previousDay);

        // ASSERT
        func.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToSemerkandPrayerTimes_UnambiguousTimeWithNonAdjacentPreviousDay_ReturnsInstantWithoutException()
    {
        // ARRANGE
        // invalid "previous day" but no ambiguity to resolve → the usage error stays irrelevant
        SemerkandDailyPrayerTimes previousDay =
            previousDayWithFajr(new LocalDate(2024, 10, 24), fajr: Instant.FromUtc(2024, 10, 24, 1, 28, 0));
        SemerkandPrayerTimesResponseDTO dto = dtoWithFajr(new LocalDate(2024, 10, 27), fajr: new LocalTime(6, 0));

        // ACT
        SemerkandDailyPrayerTimes result = convert(dto, previousDay);

        // ASSERT
        result.Fajr.Should().Be(Instant.FromUtc(2024, 10, 27, 5, 0, 0));
    }

    [Fact]
    public void ToSemerkandPrayerTimes_AmbiguousTimeWithEquidistantOptions_ReturnsEarlierInstant()
    {
        // ARRANGE
        // expected time 01:00 UTC lies exactly 30 minutes from both options → tie breaks to earlier.
        // (cannot occur with real data because the daily drift is only a few minutes)
        SemerkandDailyPrayerTimes previousDay =
            previousDayWithFajr(new LocalDate(2024, 10, 26), fajr: Instant.FromUtc(2024, 10, 26, 1, 0, 0));
        SemerkandPrayerTimesResponseDTO dto = dtoWithFajr(new LocalDate(2024, 10, 27), fajr: new LocalTime(2, 30));

        // ACT
        SemerkandDailyPrayerTimes result = convert(dto, previousDay);

        // ASSERT
        result.Fajr.Should().Be(Instant.FromUtc(2024, 10, 27, 0, 30, 0));
    }
}
