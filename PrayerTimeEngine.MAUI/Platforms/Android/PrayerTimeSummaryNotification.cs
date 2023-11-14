using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.TimeZones;
using PrayerTimeEngine.Core.Data.Preferences;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;
using System.Timers;

namespace PrayerTimeEngine.Platforms.Android
{
    [Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeDataSync, Enabled = true)]
    public class PrayerTimeSummaryNotification : Service
    {
        internal static string CHANNEL_ID = "prayer_time_channel";
        private const int TIMER_FREQUENCY_MS = 1_000;
        private const int notificationId = 1000;

        private readonly System.Timers.Timer updateTimer;
        private readonly PreferenceService _preferenceService;

        public PrayerTimeSummaryNotification()
        {
            _preferenceService = MauiProgram.ServiceProvider.GetService<PreferenceService>();

            updateTimer = new System.Timers.Timer(TIMER_FREQUENCY_MS);
            updateTimer.Elapsed += (sender, e) => UpdateNotification();

            // hack to make sure that the timer starts at a round second
            Task.Run(() => 
            {
                Thread.Sleep(1000 - DateTime.Now.Millisecond);
                updateTimer.Start();
            });
        }

        public override IBinder OnBind(Intent intent) => throw new NotImplementedException();

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var initialNotification = GetNotificationBuilder("Loading...").Build();

            if (OperatingSystem.IsAndroidVersionAtLeast(29))
                StartForeground(notificationId, initialNotification, global::Android.Content.PM.ForegroundService.TypeDataSync);
            else
                StartForeground(notificationId, initialNotification);

            return StartCommandResult.Sticky;
        }

        private void UpdateNotification()
        {
            var notificationBuilder = GetNotificationBuilder(getRemainingTimeText());

            var context = global::Android.App.Application.Context;
            var notificationManager = context.GetSystemService(NotificationService) as NotificationManager;
            notificationManager.Notify(notificationId, notificationBuilder.Build());
        }

        private Notification.Builder GetNotificationBuilder(string remainingTimeText)
        {
            string title = "PrayerTimeEngine";

            var context = global::Android.App.Application.Context;
            Intent intent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName);
            PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable);
            
            var notificationBuilder = new Notification.Builder(context, CHANNEL_ID)
                .SetContentTitle(title)
                .SetContentText(remainingTimeText)
                .SetContentIntent(pendingIntent)
                .SetSmallIcon(_Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.abc_text_select_handle_middle_mtrl)
                .SetOngoing(true);

            return notificationBuilder;
        }

        private string getRemainingTimeText()
        {
            // potential for performance improvement

            if (_preferenceService.GetCurrentProfile() is not Profile profile
                || _preferenceService.GetCurrentData(profile) is not PrayerTimesBundle prayerTimeBundle)
            {
                return "-";
            }

            var now = DateTime.Now
                .ToLocalDateTime()
                .InZone(DateTimeZoneProviders.Tzdb[TimeZoneInfo.Local.Id], Resolvers.StrictResolver);

            ZonedDateTime? nextTime = null;
            string returnText = "-";

            foreach (PrayerTime prayerTime in prayerTimeBundle.AllPrayerTimes)
            {
                if (prayerTime is DuhaPrayerTime)
                    continue;

                if (prayerTime.End != null
                    && now.ToInstant() < prayerTime.End.Value.ToInstant()
                    && (nextTime == null || prayerTime.End.Value.ToInstant() < nextTime.Value.ToInstant()))
                {
                    nextTime = prayerTime.End;
                    returnText = $"{(nextTime.Value - now).ToString("HH:mm:ss", null)} until {prayerTime.Name}-End ({nextTime.Value.ToString("HH:mm:ss", null)})";
                }

                if (prayerTime.Start != null
                    && now.ToInstant() < prayerTime.Start.Value.ToInstant()
                    && (nextTime == null || prayerTime.Start.Value.ToInstant() < nextTime.Value.ToInstant()))
                {
                    nextTime = prayerTime.Start;
                    returnText = $"{(nextTime.Value - now).ToString("HH:mm:ss", null)} until {prayerTime.Name}-Start ({nextTime.Value.ToString("HH:mm:ss", null)})";
                }
            }

            if (nextTime == null)
                return "-";

            return returnText;
        }
    }
}
