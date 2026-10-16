using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Domain.PrayerTimeProgress;

/// <summary>
/// How far the current prayer time (or the gap between two of them) has advanced, plus the bar
/// showing it. The section lengths of that bar add up to <see cref="PrayerTimeProgressCalculator.PROGRESS_MAX"/>.
/// </summary>
/// <param name="ElapsedPercent">Share of the span which has already passed.</param>
/// <param name="SegmentLengths">Length of each section in percent, adding up to 100.</param>
/// <param name="Labels">Name of each transition between two sections, so one less than there are sections.</param>
/// <param name="Start">Start of the prayer time, or end of the previous one in gap mode.</param>
/// <param name="End">End of the prayer time, or start of the next one in gap mode.</param>
/// <param name="Section">The running prayer time, or the next one in gap mode.</param>
/// <param name="PreviousSection">Only in gap mode: the prayer time which ended at <paramref name="Start"/>.</param>
public sealed record PrayerTimeProgressInfo(
    int ElapsedPercent,
    int[] SegmentLengths,
    string[] Labels,
    ZonedDateTime Start,
    ZonedDateTime End,
    ETimeSection Section,
    ETimeSection? PreviousSection = null)
{
    public bool IsGapBetweenTimes => PreviousSection is not null;
}

public static class PrayerTimeProgressCalculator
{
    /// <summary>Everything is expressed as a share of the prayer time, so the bar never needs its length.</summary>
    public const int PROGRESS_MAX = 100;

    /// <summary>
    /// Progress of the prayer time which currently contains <paramref name="now"/>.
    /// <para>
    /// Prayer times overlap, so the shortest window containing <paramref name="now"/> wins - that is
    /// the most specific information there is. When no window contains it, e.g. between Fajr-End and
    /// Duha-Start, the progress through the gap between the two neighbouring times is returned instead.
    /// <c>null</c> only when not even that can be determined.
    /// </para>
    /// </summary>
    public static PrayerTimeProgressInfo? Calculate(IEnumerable<PrayerTimeWindow> windows, Instant now)
    {
        List<PrayerTimeWindow> windowList = [.. windows];

        PrayerTimeWindow? currentTime = null;

        foreach (PrayerTimeWindow window in windowList)
        {
            if (window.Start is null || window.End is null)
            {
                continue;
            }

            if (now < window.Start.Value.ToInstant() || now >= window.End.Value.ToInstant())
            {
                continue;
            }

            // prioritize the shorter one when two times overlap
            if (currentTime is null || GetDuration(window) < GetDuration(currentTime))
            {
                currentTime = window;
            }
        }

        if (currentTime?.Start is null || currentTime.End is null)
        {
            return GetGapProgress(now, windowList);
        }

        var start = currentTime.Start.Value.ToInstant();
        double totalSeconds = GetDuration(currentTime).TotalSeconds;

        if (totalSeconds <= 0)
        {
            return null;
        }

        int elapsedPercent = (int)Math.Clamp((now - start).TotalSeconds / totalSeconds * PROGRESS_MAX, 0, PROGRESS_MAX);

        (int[] segmentLengths, string[] labels) = GetBar(currentTime, totalSeconds);

        return new PrayerTimeProgressInfo(
            elapsedPercent,
            segmentLengths,
            labels,
            currentTime.Start.Value,
            currentTime.End.Value,
            currentTime.Section);
    }

    /// <summary>
    /// Progress through the gap between two prayer times: it starts where the previous one ended and
    /// ends where the next one begins, so it fits the very same bar.
    /// </summary>
    private static PrayerTimeProgressInfo? GetGapProgress(Instant now, List<PrayerTimeWindow> windows)
    {
        ZonedDateTime? previousEnd = null;
        ZonedDateTime? nextStart = null;
        ETimeSection previousSection = default;
        ETimeSection nextSection = default;

        foreach (PrayerTimeWindow window in windows)
        {
            if (window.End?.ToInstant() <= now
                && (previousEnd is null || previousEnd.Value.ToInstant() < window.End.Value.ToInstant()))
            {
                previousEnd = window.End;
                previousSection = window.Section;
            }

            if (now < window.Start?.ToInstant()
                && (nextStart is null || window.Start.Value.ToInstant() < nextStart.Value.ToInstant()))
            {
                nextStart = window.Start;
                nextSection = window.Section;
            }
        }

        if (previousEnd is null || nextStart is null)
        {
            return null;
        }

        double totalSeconds = (nextStart.Value.ToInstant() - previousEnd.Value.ToInstant()).TotalSeconds;

        if (totalSeconds <= 0)
        {
            return null;
        }

        int elapsedPercent = (int)Math.Clamp(
            (now - previousEnd.Value.ToInstant()).TotalSeconds / totalSeconds * PROGRESS_MAX,
            0,
            PROGRESS_MAX);

        return new PrayerTimeProgressInfo(
            ElapsedPercent: elapsedPercent,
            SegmentLengths: [PROGRESS_MAX],
            Labels: [],
            Start: previousEnd.Value,
            End: nextStart.Value,
            Section: nextSection,
            PreviousSection: previousSection);
    }

    /// <summary>
    /// All points in time at which <see cref="Calculate"/> switches to another prayer time or gap, in
    /// ascending order. Between two of them the result only differs in its elapsed share.
    /// </summary>
    /// <param name="includeSubTimes">Also split at the sub times, where the bar enters its next section.</param>
    public static List<Instant> GetTransitions(IEnumerable<PrayerTimeWindow> windows, bool includeSubTimes = false)
    {
        return [.. windows
            .SelectMany(window => includeSubTimes
                ? [window.Start, window.End, .. window.SubTimes.Select(subTime => subTime.Time)]
                : new[] { window.Start, window.End })
            .Where(time => time is not null)
            .Select(time => time!.Value.ToInstant())
            .Distinct()
            .Order()];
    }

    private static Duration GetDuration(PrayerTimeWindow window)
    {
        if (window.Start is null || window.End is null)
        {
            throw new ArgumentException("Prayer time start or end is null", nameof(window));
        }

        return window.End.Value.ToInstant() - window.Start.Value.ToInstant();
    }

    /// <summary>
    /// Splits the prayer time at its sub times. Without usable sub times the bar stays undivided.
    /// </summary>
    private static (int[] SegmentLengths, string[] Labels) GetBar(PrayerTimeWindow window, double totalSeconds)
    {
        if (window.Start is null || window.End is null)
        {
            throw new ArgumentException("Prayer time start or end is null", nameof(window));
        }

        var start = window.Start.Value.ToInstant();
        var end = window.End.Value.ToInstant();

        List<(string Name, Instant Time)> subTimes =
            [.. window.SubTimes
                    .Where(subTime => subTime.Time is not null)
                    .Select(subTime => (subTime.Name, Time: subTime.Time!.Value.ToInstant()))
                    .Where(subTime => start < subTime.Time && subTime.Time < end)
                    .OrderBy(subTime => subTime.Time)];

        if (subTimes.Count == 0)
        {
            return ([PROGRESS_MAX], []);
        }

        return (
            GetSegmentLengths(start, end, [.. subTimes.Select(subTime => subTime.Time)], totalSeconds),
            [.. subTimes.Select(subTime => subTime.Name)]);
    }

    private static int[] GetSegmentLengths(Instant start, Instant end, List<Instant> subTimes, double totalSeconds)
    {
        List<Instant> boundaries = [.. subTimes, end];

        int[] lengths = new int[boundaries.Count];
        Instant sectionStart = start;
        int assigned = 0;

        for (int i = 0; i < boundaries.Count; i++)
        {
            bool isLastSection = i == boundaries.Count - 1;

            lengths[i] = isLastSection
                ? PROGRESS_MAX - assigned // take whatever is left
                : (int)Math.Round((boundaries[i] - sectionStart).TotalSeconds / totalSeconds * PROGRESS_MAX);

            assigned += lengths[i];
            sectionStart = boundaries[i];
        }

        return lengths;
    }
}
