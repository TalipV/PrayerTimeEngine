using AndroidX.Concurrent.Futures;
using AndroidX.Wear.Tiles;
using Com.Companyname.Prayertimeengine.Wear;
using Google.Common.Util.Concurrent;
using NodaTime;
using PrayerTimeEngine.Core.Domain.PrayerTimeProgress;
using PrayerTimeEngine.Core.Domain.Wear;
using PrayerTimeEngine.Wear.Data;
using LayoutElementBuilders = AndroidX.Wear.ProtoLayout.LayoutElementBuilders;
using ResourceBuilders = AndroidX.Wear.ProtoLayout.ResourceBuilders;
using TimelineBuilders = AndroidX.Wear.ProtoLayout.TimelineBuilders;

namespace PrayerTimeEngine.Wear.Tiles;

/// <summary>
/// The "arc" tile: the running prayer time as an arc along the edge of the round display, split at
/// its sub times, with the countdown to its end in the middle.
/// <para>
/// The tile holds one timeline entry per prayer time (and gap between them). Within an entry the
/// countdown and the arc fill are dynamic expressions evaluated by the watch itself, so the tile
/// does not need to be refreshed every second - only when the times from the phone change.
/// </para>
/// </summary>
[Service(
    Exported = true,
    Name = "com.companyname.prayertimeengine.wear.PrayerTimeTileService",
    Label = "Gebetszeit",
    Icon = "@drawable/ic_prayer",
    Permission = "com.google.android.wearable.permission.BIND_TILE_PROVIDER")]
[IntentFilter(["androidx.wear.tiles.action.BIND_TILE_PROVIDER"])]
[MetaData("androidx.wear.tiles.PREVIEW", Resource = "@drawable/ic_prayer")]
public class PrayerTimeTileService : TileService
{
    private const string RESOURCES_VERSION = "1";

    /// <summary>Bounded so the tile stays small, the phone sends new times every day anyway.</summary>
    private static readonly Duration s_timelineLength = Duration.FromHours(36);

    protected override IListenableFuture OnTileRequest(RequestBuilders.TileRequest? requestParams)
    {
        TileBuilders.Tile tile = new TileBuilders.Tile.Builder()
            .SetResourcesVersion(RESOURCES_VERSION)!
            .SetTileTimeline(CreateTimeline())!
            // fallback only, new times from the phone request an update themselves
            .SetFreshnessIntervalMillis((long)TimeSpan.FromHours(1).TotalMilliseconds)!
            .Build()!;

        return Completed(tile);
    }

    protected override IListenableFuture OnTileResourcesRequest(RequestBuilders.ResourcesRequest? requestParams)
    {
        return Completed(new ResourceBuilders.Resources.Builder().SetVersion(RESOURCES_VERSION)!.Build()!);
    }

    private TimelineBuilders.Timeline CreateTimeline()
    {
        var timelineBuilder = new TimelineBuilders.Timeline.Builder();

        WearPrayerTimesPayload? payload = PrayerTimesStore.Load(this);

        if (payload is null)
        {
            return timelineBuilder
                .AddTimelineEntry(new TimelineBuilders.TimelineEntry.Builder()
                    .SetLayout(CreateLayout(CreateMessage("Öffne die App auf dem Handy")))!
                    .Build()!)!
                .Build()!;
        }

        List<PrayerTimeWindow> windows = payload.ToWindows();
        Instant now = SystemClock.Instance.GetCurrentInstant();
        Instant timelineEnd = now + s_timelineLength;

        // every transition starts a new entry, between two of them only the fill of a single section
        // of the arc changes, which the watch animates on its own
        List<Instant> transitions =
        [
            now,
            .. PrayerTimeProgressCalculator.GetTransitions(windows, includeSubTimes: true).Where(transition => now < transition && transition < timelineEnd)
        ];

        for (int i = 0; i < transitions.Count; i++)
        {
            Instant entryStart = transitions[i];
            Instant? entryEnd = i + 1 < transitions.Count ? transitions[i + 1] : null;

            PrayerTimeProgressInfo? progress = PrayerTimeProgressCalculator.Calculate(windows, entryStart);

            LayoutElementBuilders.ILayoutElement root = progress is null
                ? CreateMessage("Keine Zeiten für jetzt")
                : CreateArc(progress, payload.City);

            TimelineBuilders.TimeInterval.Builder validityBuilder = new TimelineBuilders.TimeInterval.Builder()
                .SetStartMillis(entryStart.ToUnixTimeMilliseconds())!;

            if (entryEnd is not null)
            {
                validityBuilder.SetEndMillis(entryEnd.Value.ToUnixTimeMilliseconds());
            }

            timelineBuilder.AddTimelineEntry(new TimelineBuilders.TimelineEntry.Builder()
                .SetValidity(validityBuilder.Build()!)!
                .SetLayout(CreateLayout(root))!
                .Build()!);
        }

        return timelineBuilder.Build()!;
    }

    private LayoutElementBuilders.ILayoutElement CreateArc(PrayerTimeProgressInfo progress, string city)
    {
        return PrayerTimeArcLayout.Create(
            progress.IsGapBetweenTimes
                ? $"{progress.PreviousSection} → {progress.Section}"
                : $"{progress.Section} · {city}",
            progress.IsGapBetweenTimes ? $"bis {progress.Section}" : "verbleibend",
            $"{progress.Start:HH:mm}  –  {progress.End:HH:mm}",
            progress.Start.ToInstant().ToUnixTimeSeconds(),
            progress.End.ToInstant().ToUnixTimeSeconds(),
            progress.SegmentLengths,
            progress.Labels,
            progress.ElapsedPercent,
            progress.IsGapBetweenTimes,
            PackageName!,
            MainActivity.JAVA_NAME)!;
    }

    private LayoutElementBuilders.ILayoutElement CreateMessage(string message)
    {
        return PrayerTimeArcLayout.CreateMessage(message, PackageName!, MainActivity.JAVA_NAME)!;
    }

    private static LayoutElementBuilders.Layout CreateLayout(LayoutElementBuilders.ILayoutElement root)
    {
        return LayoutElementBuilders.Layout.FromLayoutElement(root)!;
    }

    private static IListenableFuture Completed(Java.Lang.Object value)
    {
        return CallbackToFutureAdapter.GetFuture(new ImmediateResolver(value))!;
    }

    /// <summary>The tile is built synchronously, so the future it is handed over in is completed right away.</summary>
    private sealed class ImmediateResolver(Java.Lang.Object value) : Java.Lang.Object, CallbackToFutureAdapter.IResolver
    {
        public Java.Lang.Object? AttachCompleter(CallbackToFutureAdapter.Completer? completer)
        {
            completer!.Set(value);
            return nameof(ImmediateResolver);
        }
    }
}
