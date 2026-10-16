using Android.App;
using Android.Content;
using Android.OS;
using AsyncAwaitBestPractices;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.PrayerTimeProgress;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Core.Domain.Wear;
using PrayerTimeEngine.Platforms.Android.Wear;
using PrayerTimeEngine.Presentation;

namespace PrayerTimeEngine.Platforms.Android.Notifications;

[Service(
    ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeSpecialUse,
    Enabled = true,
    Exported = true)]
public class PrayerTimeSummaryNotification : Service
{
    internal const string CHANNEL_ID = "prayer_time_channel";
    private const int TIMER_FREQUENCY_MS = 1_000;
    private const int MAXIMUM_UPDATE_WAITING_DURATION_MS = 5_000;
    private const int NotificationId = 1000;

    private readonly System.Timers.Timer _updateTimer;

    private readonly IProfileService _profileService;
    private readonly IDynamicPrayerTimeProviderManager _prayerTimeDynamicPrayerTimeProviderManager;
    private readonly IMosquePrayerTimeProviderManager _mosquePrayerTimeProviderManager;
    private readonly ILogger<PrayerTimeSummaryNotification> _logger;
    private readonly ISystemInfoService _systemInfoService;

    public PrayerTimeSummaryNotification()
    {
        _profileService = MauiProgram.ServiceProvider.GetRequiredService<IProfileService>();
        _prayerTimeDynamicPrayerTimeProviderManager = MauiProgram.ServiceProvider.GetRequiredService<IDynamicPrayerTimeProviderManager>();
        _mosquePrayerTimeProviderManager = MauiProgram.ServiceProvider.GetRequiredService<IMosquePrayerTimeProviderManager>();
        _logger = MauiProgram.ServiceProvider.GetRequiredService<ILogger<PrayerTimeSummaryNotification>>();
        _systemInfoService = MauiProgram.ServiceProvider.GetRequiredService<ISystemInfoService>();

        _updateTimer = new System.Timers.Timer(TIMER_FREQUENCY_MS);
        _updateTimer.Elapsed += (sender, e) => Task.Run(UpdateNotification);

        // "hack" to make sure that the timer starts at a round second
        Task.Run(() =>
        {
            Thread.Sleep(1000 - DateTime.Now.Millisecond);
            _updateTimer.Start();
        });
    }

    public override IBinder OnBind(Intent? intent) => throw new NotImplementedException();

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        Notification.Builder builder = GetNotificationBuilder();
        builder.SetContentText("Loading...");

        Notification initialNotification = builder.Build();

        try
        {
            _logger.LogInformation("Try start foreground service");

            if (OperatingSystem.IsAndroidVersionAtLeast(34))
            {
                StartForeground(NotificationId, initialNotification, global::Android.Content.PM.ForegroundService.TypeSpecialUse);
            }
            else if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                StartForeground(NotificationId, initialNotification, global::Android.Content.PM.ForegroundService.TypeNone);
            }
            else
            {
                StartForeground(NotificationId, initialNotification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during starting of foreground service");
        }

        return StartCommandResult.Sticky;
    }

    private const int TrueInt = 1;
    private const int FalseInt = 0;
    private int _isUpdateInProgress = 0;

    private async Task UpdateNotification()
    {
        // when _isUpdateInProgress equals comparand (i.e. it equals "not in progress" then set it to value (i.e. to "in progress").
        // Always return the previous value of _isUpdateInProgress
        // This is thread safer than checking and then writing in two steps
        if (Interlocked.CompareExchange(ref _isUpdateInProgress, value: TrueInt, comparand: FalseInt) == TrueInt)
        {
            // The previous value was "in progress"
            return;
        }

        // The per-second render must never do (cancellable) network work - it only reads already
        // calculated times from the manager's in-memory cache. The short timeout therefore only guards
        // the local profile read, which cannot legitimately take seconds.
        using var renderCancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromMilliseconds(MAXIMUM_UPDATE_WAITING_DURATION_MS));
        try
        {
            List<Profile> profiles = await _profileService.GetProfiles(renderCancellationTokenSource.Token);

            // potential for performance improvement
            DynamicProfile mainProfile = profiles.OfType<DynamicProfile>().OrderBy(x => x.SequenceNo).First();

            // Loading the prayer times (potentially over the network) happens off this per-second
            // render path, with its own generous timeout, so a slow first fetch can never be canceled
            // by the short render timeout above. Deduplicated so overlapping ticks don't stack.
            EnsureProfilesLoadedInBackground(profiles, mainProfile);

            Notification.Builder notificationBuilder = GetNotificationBuilder();

            if (mainProfile == null)
            {
                ApplyContent(notificationBuilder, profileInfo: "No dynamic profile", progress: null);
            }
            else
            {
                ApplyContent(
                    notificationBuilder,
                    profileInfo: mainProfile.PlaceInfo.City,
                    GetProgress(mainProfile));
            }

            Context context = global::Android.App.Application.Context;
            NotificationManager notificationManager = context.GetSystemService(NotificationService) as NotificationManager
                ?? throw new Exception("NotificationManager could not be retrieved");
            notificationManager.Notify(NotificationId, notificationBuilder.Build());
        }
        finally
        {
            // Reset _isUpdateInProgress to allow for new updates
            Interlocked.Exchange(ref _isUpdateInProgress, FalseInt);
        }
    }

    private Notification.Builder? _notificationBuilder;

    private Notification.Builder GetNotificationBuilder()
    {
        if (_notificationBuilder is null)
        {
            string title = "PrayerTimeEngine";

            Context context = global::Android.App.Application.Context;

            if (context?.PackageManager is null || context.PackageName is null)
            {
                throw new Exception("Package information could not be retrieved");
            }

            Intent intent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName)
                ?? throw new Exception("Intent could not be retrieved");

            PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable)
                ?? throw new Exception("PendingIntent could not be retrieved");

            Notification.Builder notificationBuilder = new Notification.Builder(context, CHANNEL_ID)
                .SetContentTitle(title)
                .SetContentIntent(pendingIntent)
                .SetSmallIcon(_Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.ic_notification)
                .SetColor(AppColors.AccentArgb)
                .SetOngoing(true)
                // the builder is reused for an update every second, so the notification must stay quiet
                .SetOnlyAlertOnce(true);

            _notificationBuilder = notificationBuilder;
        }

        return _notificationBuilder;
    }

    /// <summary>The sections the sub times cut the prayer time into, plus their names.</summary>
    private sealed record ProgressBarInfo(int[] SegmentLengths, string[] Labels);

    /// <summary>
    /// How far the currently running prayer time has advanced, plus the bar showing it. The section
    /// lengths of that bar add up to <see cref="PrayerTimeProgressCalculator.PROGRESS_MAX"/>.
    /// </summary>
    private sealed record CurrentTimeProgress(
        int ElapsedPercent,
        ProgressBarInfo Bar,
        string StartText,
        string EndText,
        string RemainingText,
        string SectionText,
        string? StartSubText = null,
        string? EndSubText = null,
        bool IsGapBetweenTimesMode = false);

    /// <summary>
    /// Progress of the prayer time which currently contains now, or of the gap between two of them.
    /// The calculation itself is shared with the watch, this only turns it into the texts shown here.
    /// </summary>
    private CurrentTimeProgress? GetProgress(DynamicProfile profile)
    {
        ZonedDateTime now = _profileService.GetCurrentZonedDateTime(profile);

        // read-only: never trigger a calculation here. If the times aren't calculated yet (first run,
        // or after a reboot before the background load finished) simply show no progress instead of
        // forcing a network fetch under the short render timeout.
        if (!_prayerTimeDynamicPrayerTimeProviderManager.TryGetAlreadyCalculatedPrayerTimes(
                profile.ID,
                now,
                out DynamicPrayerTimesDaySet prayerTimeBundle))
        {
            return null;
        }

        List<PrayerTimeWindow> windows = PrayerTimeWindow.FromPrayerTimes(prayerTimeBundle.AllPrayerTimes);

        SendToWearOnceAMinute(profile, now, windows);

        PrayerTimeProgressInfo? progress = PrayerTimeProgressCalculator.Calculate(windows, now.ToInstant());

        if (progress is null)
        {
            return null;
        }

        if (progress.IsGapBetweenTimes)
        {
            // both durations are spelled out underneath the two ends, since there is no prayer time
            // whose name could carry the meaning
            return new CurrentTimeProgress(
                ElapsedPercent: progress.ElapsedPercent,
                Bar: new ProgressBarInfo(progress.SegmentLengths, progress.Labels),
                StartText: progress.Start.ToString("HH:mm", null),
                EndText: progress.End.ToString("HH:mm", null),
                // the two durations below the bar say everything, a third one in the header would only repeat them
                RemainingText: string.Empty,
                SectionText: $"{progress.PreviousSection} → {progress.Section}",
                StartSubText: $"({(now - progress.Start).ToString("HH:mm:ss", null)})",
                EndSubText: $"({(progress.End - now).ToString("HH:mm:ss", null)})",
                IsGapBetweenTimesMode: true);
        }

        return new CurrentTimeProgress(
            progress.ElapsedPercent,
            new ProgressBarInfo(progress.SegmentLengths, progress.Labels),
            progress.Start.ToString("HH:mm", null),
            progress.End.ToString("HH:mm", null),
            (progress.End - now).ToString("HH:mm:ss", null),
            progress.Section.ToString());
    }

    private long _lastWearSendMinute = -1;

    /// <summary>
    /// Hands the times to the watch. Checked once a minute only, and the sender skips identical
    /// payloads, so the watch is only woken up when the times actually changed (new day, other profile).
    /// </summary>
    private void SendToWearOnceAMinute(DynamicProfile profile, ZonedDateTime now, List<PrayerTimeWindow> windows)
    {
        long minute = now.ToInstant().ToUnixTimeSeconds() / 60;

        if (minute == _lastWearSendMinute)
        {
            return;
        }

        _lastWearSendMinute = minute;

        var payload = WearPrayerTimesPayload.Create(profile.Name, profile.PlaceInfo?.City ?? string.Empty, now.Zone, windows);

        WearDataSender.SendAsync(global::Android.App.Application.Context, payload, _logger)
            .SafeFireAndForget(exception => _logger.LogError(exception, "Error while sending prayer times to the watch"));
    }

    /// <summary>
    /// Fills the notification through a custom layout instead of the standard template.
    /// <para>
    /// <c>Notification.ProgressStyle</c> draws the nicer bar, but only in the expanded notification -
    /// it had to be pulled open every single time to see anything. A custom content view is the only
    /// way to get the bar into the collapsed state, and the only way to label the sub times at all.
    /// </para>
    /// </summary>
    private static void ApplyContent(
        Notification.Builder notificationBuilder,
        string profileInfo,
        CurrentTimeProgress? progress)
    {
        // the running prayer time belongs next to the place, both name the same thing.
        // outside of any prayer time the name is simply missing, the place is not
        string title = $"{progress?.SectionText ?? "-"} @ {profileInfo}";

        // the countdown ends right above the end of the bar, whose time is written right below it
        string headline = progress?.RemainingText ?? string.Empty;

        // the custom views are what gets displayed, these two only travel along inside the
        // notification for whoever reads it without rendering it
        notificationBuilder.SetContentTitle(title);
        notificationBuilder.SetContentText(headline);

        // both states get the same content - the whole point of the custom layout is that nothing
        // requires expanding the notification first
        notificationBuilder
            .SetStyle(new Notification.DecoratedCustomViewStyle())
            .SetCustomContentView(CreateViews(title, headline, progress))
            .SetCustomBigContentView(CreateViews(title, headline, progress));
    }

    /// <summary>
    /// Builds the notification body.
    /// <para>
    /// The collapsed notification has a fixed height. Handing it a taller image does not crop that
    /// image, it scales the whole thing down - bars and text alike - until it fits. So the collapsed
    /// state gets the first division only, at full size, and the rest appears on expanding.
    /// </para>
    /// </summary>
    private static global::Android.Widget.RemoteViews CreateViews(
        string title,
        string headline,
        CurrentTimeProgress? progress)
    {
        Context context = global::Android.App.Application.Context;

        var views = new global::Android.Widget.RemoteViews(
            context.PackageName,
            _Microsoft.Android.Resource.Designer.ResourceConstant.Layout.notification_prayer_time);

        views.SetTextViewText(ResourceIds.Title, title);
        views.SetTextViewText(ResourceIds.Headline, headline);

        ApplyBar(views, progress);

        return views;
    }

    /// <summary>
    /// Draws every division of the prayer time into a single image, or hides it when no prayer time
    /// is running. They all divide the same span, so the start and end time end up underneath the
    /// whole stack instead of being repeated per bar.
    /// </summary>
    private static void ApplyBar(global::Android.Widget.RemoteViews views, CurrentTimeProgress? progress)
    {
        views.SetViewVisibility(
            ResourceIds.ProgressBar,
            progress is not null
                ? global::Android.Views.ViewStates.Visible
                : global::Android.Views.ViewStates.Gone);

        if (progress is null)
        {
            return;
        }

        views.SetImageViewBitmap(
            ResourceIds.ProgressBar,
            PrayerTimeProgressBarRenderer.Render(
                progress.ElapsedPercent,
                progress.Bar.SegmentLengths,
                progress.Bar.Labels,
                progress.StartText,
                progress.EndText,
                progress.StartSubText,
                progress.EndSubText,
                progress.IsGapBetweenTimesMode));
    }

    private static class ResourceIds
    {
        public const int Title = _Microsoft.Android.Resource.Designer.ResourceConstant.Id.notification_title;
        public const int Headline = _Microsoft.Android.Resource.Designer.ResourceConstant.Id.notification_headline;
        public const int ProgressBar = _Microsoft.Android.Resource.Designer.ResourceConstant.Id.notification_progress_bar;
    }

    private LocalDate _lastLoadedDate = new(2000, 1, 1);
    private int _isLoadInProgress = 0;

    /// <summary>
    /// Ensures the prayer times of all profiles are calculated (and thereby cached) for the current day,
    /// off the per-second render path. The actual fetch may need the network, so it runs with a generous
    /// timeout (bounded by the HttpClient/resilience pipeline) instead of the short render timeout.
    /// Deduplicated so overlapping timer ticks never stack, and retried on the next tick until the main
    /// profile is available so a failed first fetch doesn't leave the notification blank all day.
    /// </summary>
    private void EnsureProfilesLoadedInBackground(List<Profile> profiles, DynamicProfile mainProfile)
    {
        ZonedDateTime currentZonedDateTime = _systemInfoService.GetCurrentZonedDateTime();

        if (profiles.Count == 0 || _lastLoadedDate == currentZonedDateTime.Date)
        {
            return;
        }

        // when _isLoadInProgress equals "not in progress", set it to "in progress" and return the previous
        // value. If a load is already running, skip this tick instead of starting a second one.
        if (Interlocked.CompareExchange(ref _isLoadInProgress, value: TrueInt, comparand: FalseInt) == TrueInt)
        {
            return;
        }

        LoadProfilesData(profiles, mainProfile, currentZonedDateTime)
            .SafeFireAndForget(exception => _logger.LogError(exception, "Error while loading prayer time data for the notification"));
    }

    private async Task LoadProfilesData(List<Profile> profiles, DynamicProfile mainProfile, ZonedDateTime currentZonedDateTime)
    {
        try
        {
            // load the rendered (main) profile first so the notification comes alive as soon as possible
            IEnumerable<Profile> orderedProfiles = profiles.OrderByDescending(profile => ReferenceEquals(profile, mainProfile));

            foreach (Profile profile in orderedProfiles)
            {
                // No timeout token on purpose: this initial fetch may hit the network and is already
                // bounded by the HttpClient/resilience timeouts. Keeping it off the per-second render path
                // is exactly what prevents the short render timeout from canceling a slow first request.
                if (profile is DynamicProfile dynamicProfile)
                {
                    await _prayerTimeDynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(dynamicProfile.ID, currentZonedDateTime, CancellationToken.None);
                }
                else if (profile is MosqueProfile mosqueProfile)
                {
                    await _mosquePrayerTimeProviderManager.CalculatePrayerTimesAsync(mosqueProfile.ID, currentZonedDateTime, CancellationToken.None);
                }
                else
                {
                    throw new NotImplementedException($"Type of profile '{profile?.GetType().ToString() ?? "NULL"}' is not implemented");
                }
            }

            // only consider the day loaded once the profile the notification renders is actually cached,
            // so a failed fetch is retried on the next tick instead of being skipped for the rest of the day
            if (_prayerTimeDynamicPrayerTimeProviderManager.TryGetAlreadyCalculatedPrayerTimes(mainProfile.ID, currentZonedDateTime, out _))
            {
                _lastLoadedDate = currentZonedDateTime.Date;
            }
            else
            {
                _logger.LogWarning(
                    "Prayer times for the rendered profile {ProfileID} are still not available after loading for {Date}; day will be retried on the next tick",
                    mainProfile.ID,
                    currentZonedDateTime.Date);
            }
        }
        finally
        {
            Interlocked.Exchange(ref _isLoadInProgress, FalseInt);
        }
    }
}
