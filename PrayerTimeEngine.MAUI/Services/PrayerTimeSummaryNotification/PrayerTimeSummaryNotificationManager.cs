#if !WINDOWS
using Microsoft.Extensions.Logging;

namespace PrayerTimeEngine.Services
{
    public class PrayerTimeSummaryNotificationManager(
            IDispatcher dispatcher,
            ILogger<PrayerTimeSummaryNotificationManager> logger
        )
    {
        public async Task TryStartPersistentNotification()
        {
            try
            {
                if (OperatingSystem.IsAndroid())
                {
                    await tryStartPersistentNotification_Android();
                }
                else if (OperatingSystem.IsIOS())
                {
                    await tryStartPersistentNotification_iOS();
                }
            }
            catch (Exception exception) 
            {
                logger.LogError(exception, $"Error while trying to start {nameof(PrayerTimeSummaryNotification)}");
            }
        }

#if ANDROID
        private async Task tryStartPersistentNotification_Android()
        {
            bool permissionGranted = true;

            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                await dispatcher.DispatchAsync(async () =>
                {
                    await Permissions.RequestAsync<Platforms.Android.Permissions.PostNotifications>();
                });

                permissionGranted = await Permissions.CheckStatusAsync<Platforms.Android.Permissions.PostNotifications>() == PermissionStatus.Granted;
            }

            if (permissionGranted)
            {
                var startIntent = new Android.Content.Intent(global::Android.App.Application.Context, typeof(PrayerTimeSummaryNotification));
                Platforms.Android.MainActivity.Instance.StartForegroundService(startIntent);
            }
        }
#endif

        private static Task tryStartPersistentNotification_iOS()
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}
#endif
