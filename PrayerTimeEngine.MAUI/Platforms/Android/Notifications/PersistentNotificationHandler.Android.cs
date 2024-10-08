using PrayerTimeEngine.Platforms.Android.Permissions;
using PrayerTimeEngine.Services.Notifications;

namespace PrayerTimeEngine.Platforms.Android.Notifications;
public class PrayerTimeSummaryNotificationHandler : IPrayerTimeSummaryNotificationHandler
{
    public async Task ExecuteAsync()
    {
        bool permissionGranted = true;

        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            await MauiProgram.ServiceProvider.GetRequiredService<IDispatcher>().DispatchAsync(async () =>
            {
                await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<PostNotifications>();
            });

            permissionGranted = await Microsoft.Maui.ApplicationModel.Permissions.CheckStatusAsync<PostNotifications>() == PermissionStatus.Granted;
        }

        if (permissionGranted)
        {
            var startIntent =
                new global::Android.Content.Intent(
                    global::Android.App.Application.Context,
                    typeof(PrayerTimeSummaryNotification));

            MainActivity.Instance.StartForegroundService(startIntent);
        }
    }
}
