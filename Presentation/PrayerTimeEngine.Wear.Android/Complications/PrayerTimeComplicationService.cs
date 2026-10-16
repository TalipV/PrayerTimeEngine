using Android.Content;
using AndroidX.Wear.WatchFace.Complications.Data;
using AndroidX.Wear.WatchFace.Complications.Datasource;
using NodaTime;
using PrayerTimeEngine.Core.Domain.PrayerTimeProgress;
using PrayerTimeEngine.Core.Domain.Wear;
using PrayerTimeEngine.Wear.Data;
using static AndroidX.Wear.ProtoLayout.Expression.DynamicBuilders;

namespace PrayerTimeEngine.Wear.Complications;

/// <summary>
/// The prayer time on the watch face: its name, a ring for its progress and the remaining time.
/// <para>
/// Hands over a timeline with one entry per prayer time (and gap between them). The remaining time
/// is a count down text and the ring a dynamic value, both evaluated by the watch face itself, so
/// nothing needs to be pushed while a prayer time is running.
/// </para>
/// </summary>
[Service(
    Exported = true,
    Name = "com.companyname.prayertimeengine.wear.PrayerTimeComplicationService",
    Label = "Gebetszeit",
    Icon = "@drawable/ic_prayer",
    Permission = "com.google.android.wearable.permission.BIND_COMPLICATION_PROVIDER")]
[IntentFilter(["android.support.wearable.complications.ACTION_COMPLICATION_UPDATE_REQUEST"])]
[MetaData("android.support.wearable.complications.SUPPORTED_TYPES", Value = "RANGED_VALUE,SHORT_TEXT")]
// fallback for watch faces which cannot evaluate the dynamic ring value themselves
[MetaData("android.support.wearable.complications.UPDATE_PERIOD_SECONDS", Value = "300")]
public class PrayerTimeComplicationService : ComplicationDataSourceService
{
    private static readonly Duration s_timelineLength = Duration.FromHours(36);

    public override void OnComplicationRequest(ComplicationRequest request, IComplicationRequestListener listener)
    {
        WearPrayerTimesPayload? payload = PrayerTimesStore.Load(this);

        if (payload is null)
        {
            listener.OnComplicationData(CreateNoData());
            return;
        }

        List<PrayerTimeWindow> windows = payload.ToWindows();
        Instant now = SystemClock.Instance.GetCurrentInstant();
        Instant timelineEnd = now + s_timelineLength;

        ComplicationData current = Create(request.ComplicationType, PrayerTimeProgressCalculator.Calculate(windows, now));

        List<Instant> transitions = [.. PrayerTimeProgressCalculator.GetTransitions(windows)
            .Where(transition => now < transition && transition < timelineEnd)];

        List<TimelineEntry> entries = [];

        for (int i = 0; i < transitions.Count; i++)
        {
            Instant entryStart = transitions[i];
            Instant entryEnd = i + 1 < transitions.Count ? transitions[i + 1] : timelineEnd;

            entries.Add(new TimelineEntry(
                new TimeInterval(ToJavaInstant(entryStart), ToJavaInstant(entryEnd)),
                Create(request.ComplicationType, PrayerTimeProgressCalculator.Calculate(windows, entryStart))));
        }

        listener.OnComplicationDataTimeline(new ComplicationDataTimeline(current, entries));
    }

    public override ComplicationData? GetPreviewData(ComplicationType type)
    {
        return CreateData(
            type,
            section: "Asr",
            countDownTo: Java.Time.Instant.Now()!.PlusSeconds(4560)!,
            value: 52f,
            dynamicValue: null);
    }

    private ComplicationData Create(ComplicationType type, PrayerTimeProgressInfo? progress)
    {
        if (progress is null)
        {
            return CreateNoData();
        }

        return CreateData(
            type,
            section: progress.IsGapBetweenTimes ? $"→{progress.Section}" : progress.Section.ToString(),
            countDownTo: ToJavaInstant(progress.End.ToInstant()),
            value: progress.ElapsedPercent,
            dynamicValue: GetDynamicElapsedPercent(progress));
    }

    private ComplicationData CreateData(
        ComplicationType type,
        string section,
        Java.Time.Instant countDownTo,
        float value,
        IDynamicFloat? dynamicValue)
    {
        IComplicationText remaining = new TimeDifferenceComplicationText.Builder(
                TimeDifferenceStyle.ShortDualUnit!,
                new CountDownTimeReference(countDownTo))
            .Build();

        IComplicationText title = new PlainComplicationText.Builder(section).Build();
        IComplicationText description = new PlainComplicationText.Builder($"{section}, Restzeit").Build();

        if (type == ComplicationType.ShortText)
        {
            return new ShortTextComplicationData.Builder(remaining, description)
                .SetTitle(title)
                .SetTapAction(CreateOpenAppIntent())
                .Build();
        }

        RangedValueComplicationData.Builder builder = dynamicValue is not null && OperatingSystem.IsAndroidVersionAtLeast(33)
            ? new RangedValueComplicationData.Builder(dynamicValue, value, 0f, PrayerTimeProgressCalculator.PROGRESS_MAX, description)
            : new RangedValueComplicationData.Builder(value, 0f, PrayerTimeProgressCalculator.PROGRESS_MAX, description);

        return builder
            .SetTitle(title)
            .SetText(remaining)
            .SetTapAction(CreateOpenAppIntent())
            .Build();
    }

    private static ComplicationData CreateNoData()
    {
        return new NoDataComplicationData();
    }

    /// <summary>Share of the prayer time which has passed by now, evaluated by the watch face.</summary>
    private static IDynamicFloat GetDynamicElapsedPercent(PrayerTimeProgressInfo progress)
    {
        double totalSeconds = (progress.End.ToInstant() - progress.Start.ToInstant()).TotalSeconds;

        return IDynamicInstant.WithSecondsPrecision(ToJavaInstant(progress.Start.ToInstant()))!
            .DurationUntil(IDynamicInstant.PlatformTimeWithSecondsPrecision()!)!
            .ToIntSeconds()!
            .AsFloat()!
            .Times((float)(PrayerTimeProgressCalculator.PROGRESS_MAX / totalSeconds))!;
    }

    private PendingIntent CreateOpenAppIntent()
    {
        var intent = new Intent(this, typeof(MainActivity));
        return PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Immutable)!;
    }

    private static Java.Time.Instant ToJavaInstant(Instant instant)
    {
        return Java.Time.Instant.OfEpochSecond(instant.ToUnixTimeSeconds())!;
    }
}
