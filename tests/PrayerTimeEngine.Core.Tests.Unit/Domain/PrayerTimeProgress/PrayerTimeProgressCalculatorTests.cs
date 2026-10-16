using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.Models.PrayerTimes;
using PrayerTimeEngine.Core.Domain.PrayerTimeProgress;
using PrayerTimeEngine.Core.Domain.Wear;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.PrayerTimeProgress;

public class PrayerTimeProgressCalculatorTests
{
    private static readonly DateTimeZone s_zone = DateTimeZoneProviders.Tzdb["Europe/Vienna"];

    private static ZonedDateTime At(int hour, int minute)
    {
        return new LocalDateTime(2025, 6, 1, hour, minute).InZoneStrictly(s_zone);
    }

    private static List<PrayerTimeWindow> CreateDay()
    {
        return PrayerTimeWindow.FromPrayerTimes(
        [
            (ETimeSection.Fajr, new FajrPrayerTime { Start = At(3, 0), End = At(5, 0), Ghalas = At(4, 0), Karaha = At(4, 30) }),
            (ETimeSection.Duha, new DuhaPrayerTime { Start = At(5, 30), End = At(12, 0) }),
            (ETimeSection.Asr, new AsrPrayerTime { Start = At(16, 0), End = At(20, 0), Mithlayn = At(17, 0), Karaha = At(19, 0) }),
            (ETimeSection.Maghrib, new GenericPrayerTime { Start = At(19, 30), End = null }),
        ]);
    }

    [Fact]
    public void Calculate_InsidePrayerTime_ReturnsProgressAndSegmentsAtSubTimes()
    {
        // ACT
        PrayerTimeProgressInfo? progress = PrayerTimeProgressCalculator.Calculate(CreateDay(), At(18, 0).ToInstant());

        // ASSERT
        progress.Should().NotBeNull();
        progress!.Section.Should().Be(ETimeSection.Asr);
        progress.IsGapBetweenTimes.Should().BeFalse();
        progress.ElapsedPercent.Should().Be(50);
        progress.SegmentLengths.Should().Equal(25, 50, 25);
        progress.Labels.Should().Equal("Mithlayn", "Karaha");
        progress.Start.Should().Be(At(16, 0));
        progress.End.Should().Be(At(20, 0));
    }

    [Fact]
    public void Calculate_OverlappingPrayerTimes_PrefersShorterOne()
    {
        // ARRANGE
        List<PrayerTimeWindow> windows =
        [
            new(ETimeSection.Dhuhr, At(12, 0), At(16, 0), []),
            new(ETimeSection.General, At(13, 0), At(14, 0), []),
        ];

        // ACT
        PrayerTimeProgressInfo? progress = PrayerTimeProgressCalculator.Calculate(windows, At(13, 30).ToInstant());

        // ASSERT
        progress!.Section.Should().Be(ETimeSection.General);
        progress.SegmentLengths.Should().Equal(100);
    }

    [Fact]
    public void Calculate_BetweenPrayerTimes_ReturnsGapProgress()
    {
        // ACT
        PrayerTimeProgressInfo? progress = PrayerTimeProgressCalculator.Calculate(CreateDay(), At(5, 15).ToInstant());

        // ASSERT
        progress!.IsGapBetweenTimes.Should().BeTrue();
        progress.PreviousSection.Should().Be(ETimeSection.Fajr);
        progress.Section.Should().Be(ETimeSection.Duha);
        progress.ElapsedPercent.Should().Be(50);
        progress.SegmentLengths.Should().Equal(100);
        progress.Labels.Should().BeEmpty();
    }

    [Fact]
    public void Calculate_AfterLastKnownTime_ReturnsNull()
    {
        // ACT
        PrayerTimeProgressInfo? progress = PrayerTimeProgressCalculator.Calculate(CreateDay(), At(23, 0).ToInstant());

        // ASSERT
        progress.Should().BeNull();
    }

    [Fact]
    public void GetTransitions_ReturnsAllStartsAndEndsInOrder()
    {
        // ACT
        List<Instant> transitions = PrayerTimeProgressCalculator.GetTransitions(CreateDay());

        // ASSERT
        transitions.Should().Equal(
            At(3, 0).ToInstant(),
            At(5, 0).ToInstant(),
            At(5, 30).ToInstant(),
            At(12, 0).ToInstant(),
            At(16, 0).ToInstant(),
            At(19, 30).ToInstant(),
            At(20, 0).ToInstant());
    }

    [Fact]
    public void WearPayload_JsonRoundTrip_KeepsTimesAndZone()
    {
        // ARRANGE
        List<PrayerTimeWindow> windows = CreateDay();
        var payload = WearPrayerTimesPayload.Create("Profile", "Wien", s_zone, windows);

        // ACT
        List<PrayerTimeWindow> roundTripped = WearPrayerTimesPayload.FromJson(payload.ToJson())!.ToWindows();

        // ASSERT
        roundTripped.Should().HaveCount(windows.Count);
        for (int i = 0; i < windows.Count; i++)
        {
            roundTripped[i].Section.Should().Be(windows[i].Section);
            roundTripped[i].Start.Should().Be(windows[i].Start);
            roundTripped[i].End.Should().Be(windows[i].End);
            roundTripped[i].SubTimes.Should().Equal(windows[i].SubTimes);
        }
    }
}
