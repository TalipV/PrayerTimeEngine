#if ANDROID
using PrayerTimeEngine.Platforms.Android.Permissions;
using PrayerTimeEngine.Platforms.Android;
using Microsoft.Extensions.Logging;

namespace PrayerTimeEngine.Services
{
    public class PrayerTimeSummaryNotificationManager
        (
            ILogger<PrayerTimeSummaryNotificationManager> logger
        )
    {
        public async void TryStartPersistentNotification()
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

        private async Task tryStartPersistentNotification_Android()
        {
            bool permissionGranted = true;

            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Permissions.RequestAsync<PostNotifications>();
                });

                permissionGranted = await Permissions.CheckStatusAsync<PostNotifications>() == PermissionStatus.Granted;
            }

            if (permissionGranted)
            {
                var startIntent = new Android.Content.Intent(global::Android.App.Application.Context, typeof(PrayerTimeSummaryNotification));
                MainActivity.Instance.StartForegroundService(startIntent);
            }
        }

        private Task tryStartPersistentNotification_iOS()
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}
#endif