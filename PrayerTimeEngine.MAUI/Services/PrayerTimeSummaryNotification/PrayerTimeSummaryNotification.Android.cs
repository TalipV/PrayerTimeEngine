using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Services.PrayerTimeSummaryNotification
{
    [Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeDataSync, Enabled = true)]
    public class PrayerTimeSummaryNotification : Service
    {
        internal const string CHANNEL_ID = "prayer_time_channel";
        private const int TIMER_FREQUENCY_MS = 1_000;
        private const int MAXIMUM_UPDATE_WAITING_DURATION_MS = 5_000;
        private const int notificationId = 1000;

        private readonly System.Timers.Timer updateTimer;

        private readonly IProfileService _profileService;
        private readonly IDynamicPrayerTimeProviderManager _prayerTimeDynamicPrayerTimeProviderManager;
        private readonly ILogger<PrayerTimeSummaryNotificationManager> _prayerTimeSummaryNotificationManager;
        private readonly ISystemInfoService _systemInfoService;

        public PrayerTimeSummaryNotification()
        {
            _profileService = MauiProgram.ServiceProvider.GetRequiredService<IProfileService>();
            _prayerTimeDynamicPrayerTimeProviderManager = MauiProgram.ServiceProvider.GetRequiredService<IDynamicPrayerTimeProviderManager>();
            _prayerTimeSummaryNotificationManager = MauiProgram.ServiceProvider.GetRequiredService<ILogger<PrayerTimeSummaryNotificationManager>>();
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

        public override IBinder OnBind(Intent intent) => throw new NotImplementedException();

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var builder = GetNotificationBuilder();
            builder.SetContentText("Loading...");

            var initialNotification = builder.Build();

            try
            {
                _prayerTimeSummaryNotificationManager.LogInformation("Try start foreground service");

                if (OperatingSystem.IsAndroidVersionAtLeast(29))
                    StartForeground(notificationId, initialNotification, Android.Content.PM.ForegroundService.TypeDataSync);
                else
                    StartForeground(notificationId, initialNotification);
            }
            catch (Exception ex)
            {
                _prayerTimeSummaryNotificationManager.LogError(ex, "Error during starting of foreground service");
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
                    var notificationBuilder = GetNotificationBuilder();
                    notificationBuilder.SetContentTitle((await _profileService.GetProfiles(default)).First().PlaceInfo.City);
                    notificationBuilder.SetContentText(await getRemainingTimeText(cancellationTokenSource.Token));

                    var context = Android.App.Application.Context;
                    var notificationManager = context.GetSystemService(NotificationService) as NotificationManager;
                    notificationManager.Notify(notificationId, notificationBuilder.Build());
                }
                finally
                {
                    // Reset isUpdateInProgress to allow for new updates
                    Interlocked.Exchange(ref isUpdateInProgress, falseInt);
                }
            }
        }

        private Notification.Builder _notificationBuilder;

        private Notification.Builder GetNotificationBuilder()
        {
            if (_notificationBuilder is null)
            {
                string title = "PrayerTimeEngine";

                var context = Android.App.Application.Context;
                Intent intent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName);
                PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable);

                var notificationBuilder = new Notification.Builder(context, CHANNEL_ID)
                    .SetContentTitle(title)
                    .SetContentIntent(pendingIntent)
                    .SetSmallIcon(_Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.abc_text_select_handle_middle_mtrl)
                    .SetOngoing(true);

                _notificationBuilder = notificationBuilder;
            }

            return _notificationBuilder;
        }

        private async Task<string> getRemainingTimeText(CancellationToken cancellationToken)
        {
            // potential for performance improvement
            Profile profile = (await _profileService.GetProfiles(cancellationToken)).First();

            ZonedDateTime now =
                _systemInfoService.GetCurrentInstant()
                    .InZone(DateTimeZoneProviders.Tzdb[profile.PlaceInfo.TimezoneInfo.Name]);

            PrayerTimesBundle prayerTimeBundle = 
                await _prayerTimeDynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(
                    profile.ID, 
                    now, 
                    cancellationToken);

            ZonedDateTime? nextTime = null;
            string timeName = "-";
            string additionalInfo = string.Empty;
            PrayerTime _lastTime = null;

            foreach (PrayerTime prayerTime in prayerTimeBundle.AllPrayerTimes)
            {
                if (prayerTime is DuhaPrayerTime)
                    continue;

                if (prayerTime.End is not null
                    && now.ToInstant() < prayerTime.End.Value.ToInstant()
                    && (nextTime is null || prayerTime.End.Value.ToInstant() < nextTime.Value.ToInstant()))
                {
                    nextTime = prayerTime.End;
                    timeName = $"{prayerTime.Name}-End";
                }

                if (prayerTime.Start is not null
                    && now.ToInstant() < prayerTime.Start.Value.ToInstant()
                    && (nextTime is null || prayerTime.Start.Value.ToInstant() < nextTime.Value.ToInstant()))
                {
                    nextTime = prayerTime.Start;
                    timeName = $"{prayerTime.Name}-Start";

                    if (_lastTime is not null && _lastTime.End is not null)
                    {
                        additionalInfo = $"{(now - _lastTime.End.Value).ToString("HH:mm:ss", null)} since {_lastTime.Name}-End";
                    }
                }

                _lastTime = prayerTime;
            }

            if (nextTime is null)
                return "-";

            return $"{(nextTime.Value - now).ToString("HH:mm:ss", null)} until {timeName} ({nextTime.Value.ToString("HH:mm:ss", null)}) {additionalInfo}";
        }
    }
}
