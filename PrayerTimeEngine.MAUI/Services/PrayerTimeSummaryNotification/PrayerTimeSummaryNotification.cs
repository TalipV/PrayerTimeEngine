﻿using Android.App;
using Android.Content;
using Android.OS;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.TimeZones;
using PrayerTimeEngine.Core.Domain.CalculationManager;
using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Services
{
    [Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeDataSync, Enabled = true)]
    public class PrayerTimeSummaryNotification : Service
    {
        internal static string CHANNEL_ID = "prayer_time_channel";
        private const int TIMER_FREQUENCY_MS = 1_000;
        private const int notificationId = 1000;

        private readonly System.Timers.Timer updateTimer;

        private readonly IProfileService _profileService;
        private readonly IPrayerTimeCalculationManager _prayerTimeCalculationManager;

        public PrayerTimeSummaryNotification()
        {
            _profileService = MauiProgram.ServiceProvider.GetService<IProfileService>();
            _prayerTimeCalculationManager = MauiProgram.ServiceProvider.GetService<IPrayerTimeCalculationManager>();

            updateTimer = new System.Timers.Timer(TIMER_FREQUENCY_MS);
            updateTimer.Elapsed += (sender, e) => Task.Run(UpdateNotification);

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
                StartForeground(notificationId, initialNotification, Android.Content.PM.ForegroundService.TypeDataSync);
            else
                StartForeground(notificationId, initialNotification);

            return StartCommandResult.Sticky;
        }

        private async Task UpdateNotification()
        {
            var notificationBuilder = GetNotificationBuilder(await getRemainingTimeText());

            var context = Android.App.Application.Context;
            var notificationManager = context.GetSystemService(NotificationService) as NotificationManager;
            notificationManager.Notify(notificationId, notificationBuilder.Build());
        }

        private Notification.Builder GetNotificationBuilder(string remainingTimeText)
        {
            string title = "PrayerTimeEngine";

            var context = Android.App.Application.Context;
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

        private LocalDate? _previousCalculationDate = null;
        private Profile _previousProfile = null;
        private PrayerTimesBundle _previousPrayerTimeBundle = null;

        private async Task<string> getRemainingTimeText()
        {
            // potential for performance improvement

            var now = DateTime.Now
                .ToLocalDateTime()
                .InZone(DateTimeZoneProviders.Tzdb[TimeZoneInfo.Local.Id], Resolvers.StrictResolver);

            Profile profile = (await _profileService.GetProfiles()).First();
            PrayerTimesBundle prayerTimeBundle;

            if (_previousCalculationDate == null || _previousProfile == null || _previousPrayerTimeBundle == null
                || _previousCalculationDate != now.Date || !_profileService.EqualsFullProfile(_previousProfile, profile))
            {
                // recalculate
                prayerTimeBundle = await _prayerTimeCalculationManager.CalculatePrayerTimesAsync(profile, now.Date);
            }
            else
            {
                // use cached values
                _previousCalculationDate = now.Date;
                _previousProfile = profile;
                prayerTimeBundle = _previousPrayerTimeBundle;
            }
            
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
