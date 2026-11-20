using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.DTOs;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.Entities;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Waqti;

public class WaqtiDailyPrayerTimesTests
{
    private static WaqtiDailyPrayerTimes CreatePrayerTimes() =>
        new()
        {
            Date = new LocalDate(2023, 7, 30),
            TimeZone = TestDataHelper.EUROPE_VIENNA_TIME_ZONE,
            Longitude = 11.4041024M,
            Latitude = 47.2692124M,
            Method = "Mwl2007",
            Fajr = Instant.FromUtc(2023, 7, 30, 1, 28, 0),
            Shuruq = Instant.FromUtc(2023, 7, 30, 3, 47, 0),
            Duha = Instant.FromUtc(2023, 7, 30, 4, 24, 0),
            Dhuhr = Instant.FromUtc(2023, 7, 30, 11, 25, 0),
            Asr = Instant.FromUtc(2023, 7, 30, 15, 29, 0),
            AsrMithlayn = Instant.FromUtc(2023, 7, 30, 16, 37, 0),
            Maghrib = Instant.FromUtc(2023, 7, 30, 18, 53, 0),
            Isha = Instant.FromUtc(2023, 7, 30, 21, 1, 0),
            NextFajr = Instant.FromUtc(2023, 7, 31, 1, 30, 0),
        };

    public static TheoryData<ETimeType, LocalDateTime> SupportedTimeTypes => new()
    {
        { ETimeType.FajrStart, new LocalDateTime(2023, 7, 30, 3, 28) },
        { ETimeType.FajrGhalas, new LocalDateTime(2023, 7, 30, 3, 28) },
        { ETimeType.FajrKaraha, new LocalDateTime(2023, 7, 30, 3, 28) },
        { ETimeType.FajrEnd, new LocalDateTime(2023, 7, 30, 5, 47) },
        { ETimeType.DuhaStart, new LocalDateTime(2023, 7, 30, 6, 24) },
        { ETimeType.DhuhrStart, new LocalDateTime(2023, 7, 30, 13, 25) },
        { ETimeType.DhuhrEnd, new LocalDateTime(2023, 7, 30, 17, 29) },
        { ETimeType.AsrStart, new LocalDateTime(2023, 7, 30, 17, 29) },
        { ETimeType.AsrEnd, new LocalDateTime(2023, 7, 30, 20, 53) },
        { ETimeType.AsrMithlayn, new LocalDateTime(2023, 7, 30, 18, 37) },
        { ETimeType.AsrKaraha, new LocalDateTime(2023, 7, 30, 23, 1) },
        { ETimeType.MaghribStart, new LocalDateTime(2023, 7, 30, 20, 53) },
        { ETimeType.MaghribEnd, new LocalDateTime(2023, 7, 30, 23, 1) },
        { ETimeType.MaghribIshtibak, new LocalDateTime(2023, 7, 30, 23, 1) },
        { ETimeType.IshaStart, new LocalDateTime(2023, 7, 30, 23, 1) },
        { ETimeType.IshaEnd, new LocalDateTime(2023, 7, 31, 3, 30) },
    };

    [Theory]
    [MemberData(nameof(SupportedTimeTypes))]
    public void GetZonedDateTimeForTimeType_SupportedTimeType_LocalTimeInTimeZoneOfTheDay(ETimeType timeType, LocalDateTime expectedLocalDateTime)
    {
        // ARRANGE
        WaqtiDailyPrayerTimes prayerTimes = CreatePrayerTimes();

        // ACT
        ZonedDateTime? result = prayerTimes.GetZonedDateTimeForTimeType(timeType);

        // ASSERT
        result.Should().NotBeNull();
        result.Value.Zone.Should().Be(TestDataHelper.EUROPE_VIENNA_TIME_ZONE);
        result.Value.LocalDateTime.Should().Be(expectedLocalDateTime);
    }

    [Theory]
    [InlineData(ETimeType.QiblaTime)]
    [InlineData(ETimeType.DuhaEnd)]
    [InlineData(ETimeType.IshaMidnight)]
    public void GetZonedDateTimeForTimeType_UnsupportedTimeType_ThrowsArgumentException(ETimeType timeType)
    {
        // ARRANGE
        WaqtiDailyPrayerTimes prayerTimes = CreatePrayerTimes();

        // ACT
        Action action = () => prayerTimes.GetZonedDateTimeForTimeType(timeType);

        // ASSERT
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetZonedDateTimeForTimeType_TimeMissing_ReturnsNull()
    {
        // ARRANGE
        WaqtiDailyPrayerTimes prayerTimes = CreatePrayerTimes();
        prayerTimes.Isha = null;

        // ACT
        ZonedDateTime? result = prayerTimes.GetZonedDateTimeForTimeType(ETimeType.IshaStart);

        // ASSERT
        result.Should().BeNull();
    }

    [Fact]
    public void ToWaqtiPrayerTimes_TimesWithFractionsOfSecondsAndMissingTimes_SecondsTruncatedAndMissingTimesNull()
    {
        // ARRANGE
        var dto = new WaqtiDailyPrayerTimesResponseDTO
        {
            Date = new LocalDate(2023, 7, 30),
            Timezone = TestDataHelper.EUROPE_VIENNA_TIME_ZONE,
            Method = "Mwl2007",
            Fajr = new WaqtiTimeResponseDTO { Instant = new DateTimeOffset(2023, 7, 30, 1, 28, 12, 750, TimeSpan.Zero), RawInstant = new DateTimeOffset(2023, 7, 30, 1, 30, 3, TimeSpan.Zero), Validity = "Valid" },
            Ishaa = new WaqtiTimeResponseDTO { Instant = null, Validity = "Invalid" },
            Maghrib = null,
        };

        // ACT
        WaqtiDailyPrayerTimes result = dto.ToWaqtiPrayerTimes(longitude: 1M, latitude: 2M);

        // ASSERT (without angle overrides the published time incl. safety minutes and rounding is taken)
        result.Fajr.Should().Be(Instant.FromUtc(2023, 7, 30, 1, 28, 12));
        result.FajrAngle.Should().BeNull();
        result.IshaaAngle.Should().BeNull();
        result.Isha.Should().BeNull();
        result.Maghrib.Should().BeNull();
        result.Longitude.Should().Be(1M);
        result.Latitude.Should().Be(2M);
    }

    [Fact]
    public void ToWaqtiPrayerTimes_NoTimeZone_ThrowsInvalidOperationException()
    {
        // ARRANGE
        var dto = new WaqtiDailyPrayerTimesResponseDTO
        {
            Date = new LocalDate(2023, 7, 30),
            Timezone = null,
        };

        // ACT
        Action action = () => dto.ToWaqtiPrayerTimes(longitude: 1M, latitude: 2M);

        // ASSERT
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToWaqtiPrayerTimes_WithAngleOverrides_RawTimesTakenAndAnglesStored()
    {
        // ARRANGE
        var dto = new WaqtiDailyPrayerTimesResponseDTO
        {
            Date = new LocalDate(2023, 7, 30),
            Timezone = TestDataHelper.EUROPE_VIENNA_TIME_ZONE,
            Method = "Mwl2007",
            Fajr = new WaqtiTimeResponseDTO { Instant = new DateTimeOffset(2023, 7, 30, 3, 0, 0, TimeSpan.Zero), RawInstant = new DateTimeOffset(2023, 7, 30, 3, 2, 30, 400, TimeSpan.Zero), Validity = "Valid" },
            Ishaa = new WaqtiTimeResponseDTO { Instant = new DateTimeOffset(2023, 7, 30, 19, 59, 0, TimeSpan.Zero), RawInstant = new DateTimeOffset(2023, 7, 30, 19, 56, 51, TimeSpan.Zero), Validity = "Valid" },
        };

        // ACT
        WaqtiDailyPrayerTimes result = dto.ToWaqtiPrayerTimes(longitude: 1M, latitude: 2M, fajrAngle: 7.5, ishaaAngle: 10);

        // ASSERT
        result.FajrAngle.Should().Be(7.5);
        result.IshaaAngle.Should().Be(10);
        result.Fajr.Should().Be(Instant.FromUtc(2023, 7, 30, 3, 2, 30));
        result.Isha.Should().Be(Instant.FromUtc(2023, 7, 30, 19, 56, 51));
    }

    [Theory]
    [InlineData("ValidIjtihad")]
    [InlineData("Invalid")]
    [InlineData(null)]
    public void ToWaqtiPrayerTimes_WithAngleOverridesAndEstimatedTime_TimeDropped(string? validity)
    {
        // ARRANGE (e.g. in the far north, where the sun does not reach the angle in summer)
        var dto = new WaqtiDailyPrayerTimesResponseDTO
        {
            Date = new LocalDate(2024, 6, 21),
            Timezone = DateTimeZoneProviders.Tzdb["Europe/Oslo"],
            Method = "Mwl2007",
            Fajr = new WaqtiTimeResponseDTO { Instant = new DateTimeOffset(2024, 6, 21, 2, 8, 0, TimeSpan.Zero), RawInstant = new DateTimeOffset(2024, 6, 21, 2, 8, 29, TimeSpan.Zero), Validity = validity },
        };

        // ACT
        WaqtiDailyPrayerTimes result = dto.ToWaqtiPrayerTimes(longitude: 18.9553M, latitude: 69.6492M, fajrAngle: 7.5, ishaaAngle: null);

        // ASSERT
        result.Fajr.Should().BeNull();
    }

    [Fact]
    public void ToWaqtiPrayerTimes_WithoutAngleOverridesAndEstimatedTime_PublishedTimeKept()
    {
        // ARRANGE (the regular times of a method keep Waqti's own high latitude handling)
        var dto = new WaqtiDailyPrayerTimesResponseDTO
        {
            Date = new LocalDate(2024, 6, 21),
            Timezone = DateTimeZoneProviders.Tzdb["Europe/Oslo"],
            Method = "Mwl2007",
            Fajr = new WaqtiTimeResponseDTO { Instant = new DateTimeOffset(2024, 6, 21, 0, 22, 0, TimeSpan.Zero), RawInstant = new DateTimeOffset(2024, 6, 21, 0, 24, 10, TimeSpan.Zero), Validity = "ValidIjtihad" },
        };

        // ACT
        WaqtiDailyPrayerTimes result = dto.ToWaqtiPrayerTimes(longitude: 18.9553M, latitude: 69.6492M);

        // ASSERT
        result.Fajr.Should().Be(Instant.FromUtc(2024, 6, 21, 0, 22, 0));
    }
}
