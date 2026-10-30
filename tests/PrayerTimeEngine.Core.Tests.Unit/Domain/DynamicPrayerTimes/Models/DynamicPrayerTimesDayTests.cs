using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Models;

public class DynamicPrayerTimesDayTests
{
    private static readonly ZonedDateTime s_sampleValue =
        new(Instant.FromUtc(2024, 1, 1, 5, 0, 0), DateTimeZone.Utc);

    // Independent restatement of the mapping in DynamicPrayerTimesDay.SetSpecificPrayerTimeDateTime.
    // Kept deliberately separate from the production switch so a mis-routed case (e.g. writing to
    // Fajr.End instead of Fajr.Ghalas) is caught by the two maps disagreeing.
    private static readonly Dictionary<ETimeType, Func<DynamicPrayerTimesDay, ZonedDateTime?>> s_getters = new()
    {
        [ETimeType.FajrStart] = d => d.Fajr.Start,
        [ETimeType.FajrEnd] = d => d.Fajr.End,
        [ETimeType.FajrGhalas] = d => d.Fajr.Ghalas,
        [ETimeType.FajrKaraha] = d => d.Fajr.Karaha,

        [ETimeType.DuhaStart] = d => d.Duha.Start,
        [ETimeType.DuhaEnd] = d => d.Duha.End,
        [ETimeType.DuhaQuarterOfDay] = d => d.Duha.QuarterOfDay,
        [ETimeType.DuhaHalfOfDay] = d => d.Duha.HalfOfDay,

        [ETimeType.DhuhrStart] = d => d.Dhuhr.Start,
        [ETimeType.DhuhrEnd] = d => d.Dhuhr.End,

        [ETimeType.AsrStart] = d => d.Asr.Start,
        [ETimeType.AsrEnd] = d => d.Asr.End,
        [ETimeType.AsrMithlayn] = d => d.Asr.Mithlayn,
        [ETimeType.AsrKaraha] = d => d.Asr.Karaha,

        [ETimeType.MaghribStart] = d => d.Maghrib.Start,
        [ETimeType.MaghribEnd] = d => d.Maghrib.End,
        [ETimeType.MaghribSufficientTime] = d => d.Maghrib.SufficientTime,
        [ETimeType.MaghribIshtibak] = d => d.Maghrib.Ishtibak,

        [ETimeType.IshaStart] = d => d.Isha.Start,
        [ETimeType.IshaEnd] = d => d.Isha.End,
        [ETimeType.IshaFirstThird] = d => d.Isha.FirstThirdOfNight,
        [ETimeType.IshaMidnight] = d => d.Isha.MiddleOfNight,
        [ETimeType.IshaSecondThird] = d => d.Isha.SecondThirdOfNight,

        [ETimeType.QiblaTime] = d => d.Qibla.Start,
    };

    public static TheoryData<ETimeType> MappedTimeTypes
    {
        get
        {
            var data = new TheoryData<ETimeType>();
            foreach (ETimeType timeType in s_getters.Keys)
            {
                data.Add(timeType);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(MappedTimeTypes))]
    public void SetSpecificPrayerTimeDateTime_SetsOnlyTheMatchingProperty(ETimeType timeType)
    {
        // ARRANGE
        var day = new DynamicPrayerTimesDay();

        // ACT
        day.SetSpecificPrayerTimeDateTime(timeType, s_sampleValue);

        // ASSERT
        s_getters[timeType](day).Should().Be(s_sampleValue);

        foreach ((ETimeType otherTimeType, Func<DynamicPrayerTimesDay, ZonedDateTime?> otherGetter) in s_getters)
        {
            if (otherTimeType == timeType)
            {
                continue;
            }

            otherGetter(day).Should().BeNull($"setting {timeType} must not affect {otherTimeType}");
        }
    }

    [Fact]
    public void SetSpecificPrayerTimeDateTime_MappingTestCoversEveryTimeType()
    {
        // guards against a new ETimeType being added to the production switch without a
        // matching case here, which would otherwise leave that value silently untested
        s_getters.Keys.Should().BeEquivalentTo(Enum.GetValues<ETimeType>());
    }

    [Fact]
    public void SetSpecificPrayerTimeDateTime_UnknownTimeType_ThrowsNotImplementedException()
    {
        // ARRANGE
        var day = new DynamicPrayerTimesDay();

        // ACT
        Action act = () => day.SetSpecificPrayerTimeDateTime((ETimeType)(-1), s_sampleValue);

        // ASSERT
        act.Should().Throw<NotImplementedException>();
    }
}
