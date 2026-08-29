using Android.App;
using Android.Content;
using Android.OS;
using AsyncAwaitBestPractices;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.Models.PrayerTimes;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
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
    private const int notificationId = 1000;

    private readonly System.Timers.Timer updateTimer;

    private readonly IProfileService _profileService;
    private readonly IDynamicPrayerTimeProviderManager _prayerTimeDynamicPrayerTimeProviderManager;
    private readonly ILogger<PrayerTimeSummaryNotification> _logger;
    private readonly ISystemInfoService _systemInfoService;

    public PrayerTimeSummaryNotification()
    {
        _profileService = MauiProgram.ServiceProvider.GetRequiredService<IProfileService>();
        _prayerTimeDynamicPrayerTimeProviderManager = MauiProgram.ServiceProvider.GetRequiredService<IDynamicPrayerTimeProviderManager>();
        _logger = MauiProgram.ServiceProvider.GetRequiredService<ILogger<PrayerTimeSummaryNotification>>();
        _systemInfoService = MauiProgram.ServiceProvider.GetRequiredService<ISystemInfoService>();

        updateTimer = new System.Timers.Timer(TIMER_FREQUENCY_MS);
        updateTimer.Elapsed += (sender, e) => Task.Run(UpdateNotification);

        // "hack" to make sure that the timer starts at a round second
        Task.Run(() =>
        {
            Thread.Sleep(1000 - DateTime.Now.Millisecond);
            updateTimer.Start();
        });
    }

    public override IBinder OnBind(Intent? intent) => throw new NotImplementedException();

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        var builder = getNotificationBuilder();
        builder.SetContentText("Loading...");

        var initialNotification = builder.Build();

        try
        {
            _logger.LogInformation("Try start foreground service");

            if (OperatingSystem.IsAndroidVersionAtLeast(34))
                StartForeground(notificationId, initialNotification, global::Android.Content.PM.ForegroundService.TypeSpecialUse);
            else if (OperatingSystem.IsAndroidVersionAtLeast(29))
                StartForeground(notificationId, initialNotification, global::Android.Content.PM.ForegroundService.TypeNone);
            else
                StartForeground(notificationId, initialNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during starting of foreground service");
        }

        return StartCommandResult.Sticky;
    }

    private const int trueInt = 1;
    private const int falseInt = 0;
    private int isUpdateInProgress = 0;

    private async Task UpdateNotification()
    {
        // when isUpdateInProgress equals comparand (i.e. it equals "not in progress" then set it to value (i.e. to "in progress").
        // Always return the previous value of isUpdateInProgress
        // This is thread safer than checking and then writing in two steps
        if (Interlocked.CompareExchange(ref isUpdateInProgress, value: trueInt, comparand: falseInt) == trueInt)
        {
            // The previous value was "in progress"
            return;
        }

        // wait 5 seconds at max
        using (var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromMilliseconds(MAXIMUM_UPDATE_WAITING_DURATION_MS)))
        {
            try
            {
                List<Profile> profiles = await _profileService.GetProfiles(cancellationTokenSource.Token);

                // potential for performance improvement
                DynamicProfile mainProfile = profiles.OfType<DynamicProfile>().FirstOrDefault();
                Profile[] otherProfiles = profiles.Except([mainProfile]).ToArray();

                if (otherProfiles.Length != 0)
                {
                    // add cancellation token for general shut down requests (timeout not really needed)
                    ensureSureOtherProfilesLoadedOnceADay(otherProfiles, CancellationToken.None)
                        .SafeFireAndForget(exception =>
                        {
                            _logger.LogError(exception, "Error while trying to load data of other profiles");
                        });
                }

                var notificationBuilder = getNotificationBuilder();

                if (mainProfile == null)
                {
                    applyContent(notificationBuilder, profileInfo: "No dynamic profile", progress: null);
                }
                else
                {
                    applyContent(
                        notificationBuilder,
                        profileInfo: mainProfile.PlaceInfo.City,
                        await getProgress(mainProfile, cancellationTokenSource.Token));
                }

                var context = global::Android.App.Application.Context;
                var notificationManager = context.GetSystemService(NotificationService) as NotificationManager
                    ?? throw new Exception("NotificationManager could not be retrieved");
                notificationManager.Notify(notificationId, notificationBuilder.Build());
            }
            finally
            {
                // Reset isUpdateInProgress to allow for new updates
                Interlocked.Exchange(ref isUpdateInProgress, falseInt);
            }
        }
    }

    private Notification.Builder? _notificationBuilder;

    private Notification.Builder getNotificationBuilder()
    {
        if (_notificationBuilder is null)
        {
            string title = "PrayerTimeEngine";

            var context = global::Android.App.Application.Context;

            if (context?.PackageManager is null || context.PackageName is null)
            {
                throw new Exception("Package information could not be retrieved");
            }

            Intent intent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName) 
                ?? throw new Exception("Intent could not be retrieved");

            PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable)
                ?? throw new Exception("PendingIntent could not be retrieved");

            var notificationBuilder = new Notification.Builder(context, CHANNEL_ID)
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

    /// <summary>Everything is expressed as a share of the prayer time, so the bar never needs its length.</summary>
    private const int PROGRESS_MAX = 100;

    /// <summary>The sections the sub times cut the prayer time into, plus their names.</summary>
    private sealed record ProgressBarInfo(int[] SegmentLengths, string[] Labels);

    /// <summary>
    /// How far the currently running prayer time has advanced, plus the bar showing it. The section
    /// lengths of that bar add up to <see cref="PROGRESS_MAX"/>.
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
    /// Progress of the prayer time which currently contains <paramref name="now"/>.
    /// <para>
    /// Prayer times overlap, so the shortest window containing <paramref name="now"/> wins - that is
    /// the most specific information there is. Returns <c>null</c> when no window contains it, e.g.
    /// between Fajr-End and Duha-Start, in which case no progress is shown at all.
    /// </para>
    /// </summary>
    private async Task<CurrentTimeProgress?> getProgress(DynamicProfile profile, CancellationToken cancellationToken)
    {
        ZonedDateTime now = _profileService.GetCurrentZonedDateTime(profile);

        DynamicPrayerTimesDaySet prayerTimeBundle =
            (await _prayerTimeDynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(
                profile.ID,
                now,
                cancellationToken)).DynamicPrayerTimesDaySet;

        Instant nowInstant = now.ToInstant();
        GenericPrayerTime? currentTime = null;
        ETimeSection currentSection = default;

        foreach ((ETimeSection section, GenericPrayerTime prayerTime) in prayerTimeBundle.AllPrayerTimes)
        {
            if (prayerTime?.Start is null || prayerTime.End is null)
                continue;

            if (nowInstant < prayerTime.Start.Value.ToInstant() || nowInstant >= prayerTime.End.Value.ToInstant())
                continue;

            if (currentTime is null
                // prioritize the shorter one when two times overlap
                || getDuration(prayerTime) < getDuration(currentTime))
            {
                currentTime = prayerTime;
                currentSection = section;
            }
        }

        if (currentTime?.Start is null || currentTime.End is null)
            return getGapProgress(now, prayerTimeBundle);

        Instant start = currentTime.Start.Value.ToInstant();
        double totalSeconds = getDuration(currentTime).TotalSeconds;

        if (totalSeconds <= 0)
            return null;

        int elapsedPercent = (int)Math.Clamp((nowInstant - start).TotalSeconds / totalSeconds * PROGRESS_MAX, 0, PROGRESS_MAX);

        return new CurrentTimeProgress(
            elapsedPercent,
            getBar(currentTime, totalSeconds),
            currentTime.Start.Value.ToString("HH:mm", null),
            currentTime.End.Value.ToString("HH:mm", null),
            (currentTime.End.Value - now).ToString("HH:mm:ss", null),
            currentSection.ToString());
    }

    /// <summary>
    /// Progress through the gap between two prayer times: it starts where the previous one ended and
    /// ends where the next one begins, so it fits the very same bar. Both durations are spelled out
    /// underneath the two ends, since there is no prayer time whose name could carry the meaning.
    /// </summary>
    private static CurrentTimeProgress? getGapProgress(ZonedDateTime now, DynamicPrayerTimesDaySet prayerTimeBundle)
    {
        Instant nowInstant = now.ToInstant();

        ZonedDateTime? previousEnd = null;
        ZonedDateTime? nextStart = null;
        ETimeSection previousSection = default;
        ETimeSection nextSection = default;

        foreach ((ETimeSection section, GenericPrayerTime prayerTime) in prayerTimeBundle.AllPrayerTimes)
        {
            if (prayerTime?.End?.ToInstant() <= nowInstant
                && (previousEnd is null || previousEnd.Value.ToInstant() < prayerTime.End.Value.ToInstant()))
            {
                previousEnd = prayerTime.End;
                previousSection = section;
            }

            if (nowInstant < prayerTime?.Start?.ToInstant()
                && (nextStart is null || prayerTime.Start.Value.ToInstant() < nextStart.Value.ToInstant()))
            {
                nextStart = prayerTime.Start;
                nextSection = section;
            }
        }

        if (previousEnd is null || nextStart is null)
            return null;

        double totalSeconds = (nextStart.Value.ToInstant() - previousEnd.Value.ToInstant()).TotalSeconds;

        if (totalSeconds <= 0)
            return null;

        int elapsedPercent = (int)Math.Clamp(
            (nowInstant - previousEnd.Value.ToInstant()).TotalSeconds / totalSeconds * PROGRESS_MAX,
            0,
            PROGRESS_MAX);

        return new CurrentTimeProgress(
            ElapsedPercent: elapsedPercent,
            Bar: new ProgressBarInfo([PROGRESS_MAX], []),
            StartText: previousEnd.Value.ToString("HH:mm", null),
            EndText: nextStart.Value.ToString("HH:mm", null),
            // the two durations below the bar say everything, a third one in the header would only repeat them
            RemainingText: string.Empty,
            SectionText: $"{previousSection} → {nextSection}",
            StartSubText: $"({(now - previousEnd.Value).ToString("HH:mm:ss", null)})",
            EndSubText: $"({(nextStart.Value - now).ToString("HH:mm:ss", null)})",
            IsGapBetweenTimesMode: true);
    }

    private static Duration getDuration(GenericPrayerTime prayerTime)
    {
        if (prayerTime?.Start is null || prayerTime.End is null)
            throw new ArgumentException("Prayer time start or end is null", nameof(prayerTime));

        return prayerTime.End.Value.ToInstant() - prayerTime.Start.Value.ToInstant();
    }

    /// <summary>
    /// Builds one bar per division of the prayer time, each split at the sub times of that division.
    /// A prayer time without usable sub times ends up with a single undivided bar.
    /// </summary>
    /// <summary>
    /// Splits the prayer time at its sub times. Without usable sub times the bar stays undivided.
    /// </summary>
    private static ProgressBarInfo getBar(GenericPrayerTime prayerTime, double totalSeconds)
    {
        if (prayerTime?.Start is null || prayerTime.End is null)
            throw new ArgumentException("Prayer time start or end is null", nameof(prayerTime));

        Instant start = prayerTime.Start.Value.ToInstant();
        Instant end = prayerTime.End.Value.ToInstant();

        List<(string Name, Instant Time)> subTimes =
            getSubTimes(prayerTime)
                .Select(subTime => (subTime.Name, Time: subTime.Time?.ToInstant()))
                .OfType<(string Name, Instant Time)>()
                .Where(subTime => start < subTime.Time && subTime.Time < end)
                .OrderBy(subTime => subTime.Time)
                .ToList();

        if (subTimes.Count == 0)
            return new ProgressBarInfo([PROGRESS_MAX], []);

        return new ProgressBarInfo(
            getSegmentLengths(start, end, subTimes.Select(subTime => subTime.Time).ToList(), totalSeconds),
            [.. subTimes.Select(subTime => subTime.Name)]);
    }

    private static int[] getSegmentLengths(Instant start, Instant end, List<Instant> subTimes, double totalSeconds)
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

    /// <summary>The sub times which divide a prayer time, in no particular order.</summary>
    private static IEnumerable<(string Name, ZonedDateTime? Time)> getSubTimes(GenericPrayerTime prayerTime)
    {
        return prayerTime switch
        {
            FajrPrayerTime fajr => [("Ghalas", fajr.Ghalas), ("Redness", fajr.Karaha)],
            DuhaPrayerTime duha => [("1/4", duha.QuarterOfDay), ("1/2", duha.HalfOfDay)],
            AsrPrayerTime asr => [("Mithlayn", asr.Mithlayn), ("Karaha", asr.Karaha)],
            MaghribPrayerTime maghrib => [("Sufficient", maghrib.SufficientTime), ("Ishtibak", maghrib.Ishtibak)],
            IshaPrayerTime isha =>
            [
                ("1/3", isha.FirstThirdOfNight),
                ("1/2", isha.MiddleOfNight),
                ("2/3", isha.SecondThirdOfNight)
            ],
            _ => []
        };
    }

    /// <summary>
    /// Fills the notification through a custom layout instead of the standard template.
    /// <para>
    /// <c>Notification.ProgressStyle</c> draws the nicer bar, but only in the expanded notification -
    /// it had to be pulled open every single time to see anything. A custom content view is the only
    /// way to get the bar into the collapsed state, and the only way to label the sub times at all.
    /// </para>
    /// </summary>
    private static void applyContent(
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
            .SetCustomContentView(createViews(title, headline, progress))
            .SetCustomBigContentView(createViews(title, headline, progress));
    }

    /// <summary>
    /// Builds the notification body.
    /// <para>
    /// The collapsed notification has a fixed height. Handing it a taller image does not crop that
    /// image, it scales the whole thing down - bars and text alike - until it fits. So the collapsed
    /// state gets the first division only, at full size, and the rest appears on expanding.
    /// </para>
    /// </summary>
    private static global::Android.Widget.RemoteViews createViews(
        string title,
        string headline,
        CurrentTimeProgress? progress)
    {
        var context = global::Android.App.Application.Context;

        var views = new global::Android.Widget.RemoteViews(
            context.PackageName,
            _Microsoft.Android.Resource.Designer.ResourceConstant.Layout.notification_prayer_time);

        views.SetTextViewText(ResourceIds.Title, title);
        views.SetTextViewText(ResourceIds.Headline, headline);

        applyBar(views, progress);

        return views;
    }

    /// <summary>
    /// Draws every division of the prayer time into a single image, or hides it when no prayer time
    /// is running. They all divide the same span, so the start and end time end up underneath the
    /// whole stack instead of being repeated per bar.
    /// </summary>
    private static void applyBar(global::Android.Widget.RemoteViews views, CurrentTimeProgress? progress)
    {
        views.SetViewVisibility(
            ResourceIds.ProgressBar, 
            progress is not null 
                ? global::Android.Views.ViewStates.Visible 
                : global::Android.Views.ViewStates.Gone);

        if (progress is null)
            return;

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

    private LocalDate _lastLoadedDate = new LocalDate(2000, 1, 1);

    private async Task ensureSureOtherProfilesLoadedOnceADay(Profile[] profiles, CancellationToken cancellationToken)
    {
        ZonedDateTime currentZonedDateTime = _systemInfoService.GetCurrentZonedDateTime();

        if (profiles.Length == 0 || _lastLoadedDate == currentZonedDateTime.Date)
        {
            return;
        }

        foreach (Profile profile in profiles)
        {
            if (profile is DynamicProfile dynamicProfile)
            {
                var dynamicPrayerTimeProviderManager = MauiProgram.ServiceProvider.GetRequiredService<IDynamicPrayerTimeProviderManager>();
                await dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(dynamicProfile.ID, currentZonedDateTime, cancellationToken);
            }
            else if (profile is MosqueProfile mosqueProfile)
            {
                var mosquePrayerTimeProviderManager = MauiProgram.ServiceProvider.GetRequiredService<IMosquePrayerTimeProviderManager>();
                await mosquePrayerTimeProviderManager.CalculatePrayerTimesAsync(mosqueProfile.ID, currentZonedDateTime, cancellationToken);
            }
            else
            {
                throw new NotImplementedException($"Type of profile '{profile?.GetType().ToString() ?? "NULL"}' is not implemented");
            }
        }

        _lastLoadedDate = currentZonedDateTime.Date;
    }
}
