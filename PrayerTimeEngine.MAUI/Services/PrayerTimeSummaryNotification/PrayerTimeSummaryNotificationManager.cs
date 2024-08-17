using Microsoft.Extensions.Logging;

namespace PrayerTimeEngine.Services.PrayerTimeSummaryNotification
{
    public class PrayerTimeSummaryNotificationManager(ILogger<PrayerTimeSummaryNotificationManager> logger)
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
                logger.LogError(exception, $"Error at {nameof(TryStartPersistentNotification)}");
            }
        }

        private async Task tryStartPersistentNotification_Android()
        {
#if ANDROID
            bool permissionGranted = true;

            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                // TODO refactor whole class anyway
                await MauiProgram.ServiceProvider.GetRequiredService<IDispatcher>().DispatchAsync(async () =>
                {
                    await Permissions.RequestAsync<Platforms.Android.Permissions.PostNotifications>();
                });

                permissionGranted = await Permissions.CheckStatusAsync<Platforms.Android.Permissions.PostNotifications>() == PermissionStatus.Granted;
            }

            if (permissionGranted)
            {
                var startIntent =
                    new Android.Content.Intent(
                        Android.App.Application.Context,
                        typeof(PrayerTimeSummaryNotification));
                Platforms.Android.MainActivity.Instance.StartForegroundService(startIntent);
            }
#else
            await Task.CompletedTask;
#endif
        }

        private static Task tryStartPersistentNotification_iOS()
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}
