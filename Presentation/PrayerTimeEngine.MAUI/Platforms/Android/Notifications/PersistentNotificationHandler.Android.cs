using Android.Content;
using PrayerTimeEngine.Platforms.Android.Permissions;
using PrayerTimeEngine.Services.Notifications;
using PrayerTimeEngine.Presentation.Localization;

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
            await ShowBatteryOptimizationWarningIfNeeded();

            var startIntent =
                new Intent(
                    global::Android.App.Application.Context,
                    typeof(PrayerTimeSummaryNotification));

            MainActivity.Instance.StartForegroundService(startIntent);
        }
    }

    private async Task ShowBatteryOptimizationWarningIfNeeded()
    {
        if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.M)
        {
            return;
        }

        Context context = global::Android.App.Application.Context;
        string? package = context.PackageName;

        var pm = (global::Android.OS.PowerManager)context.GetSystemService(Context.PowerService);
        bool alreadyIgnoringBatteryOptim = pm.IsIgnoringBatteryOptimizations(package);

        if (alreadyIgnoringBatteryOptim)
        {
            return;
        }

        // Show dialog in UI thread
        IDispatcher dispatcher = MauiProgram.ServiceProvider.GetRequiredService<IDispatcher>();

        await dispatcher.DispatchAsync(async () =>
        {
            bool openSettings = await Application.Current.Windows[0].Page.DisplayAlertAsync(
                title: AppResources.BatteryOptimization_Title,
                message: AppResources.BatteryOptimization_Message,
                accept: AppResources.BatteryOptimization_Accept,
                cancel: AppResources.BatteryOptimization_Decline);

            if (!openSettings)
            {
                return;
            }

            var intent = new Intent(
                global::Android.Provider.Settings.ActionIgnoreBatteryOptimizationSettings);

            intent.SetFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        });
    }
}
