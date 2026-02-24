using PrayerTimeEngine.Platforms.Android.Permissions;
using PrayerTimeEngine.Services.Notifications;

namespace PrayerTimeEngine.Platforms.Android.Notifications;

public class PrayerTimeSummaryNotificationHandler : IPrayerTimeSummaryNotificationHandler
{
    private const string BATTERY_OPTIMIZATION_WARNING_TEXT = """
                Damit die Gebetszeiten-Benachrichtigung zuverlässig funktioniert, 
                muss die App von den Akku-Optimierungen ausgenommen sein.

                Bitte wählen Sie unter Batterieeinstellungen die Option 'Unbeschränkt' bzw. 'Nicht optimieren'.
                """;

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
            await showBatteryOptimizationWarningIfNeeded();

            var startIntent =
                new global::Android.Content.Intent(
                    global::Android.App.Application.Context,
                    typeof(PrayerTimeSummaryNotification));

            MainActivity.Instance.StartForegroundService(startIntent);
        }
    }

    private async Task showBatteryOptimizationWarningIfNeeded()
    {
        if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.M)
            return;

        var context = global::Android.App.Application.Context;
        var package = context.PackageName;

        var pm = (global::Android.OS.PowerManager)context.GetSystemService(global::Android.Content.Context.PowerService);
        bool alreadyIgnoringBatteryOptim = pm.IsIgnoringBatteryOptimizations(package);

        if (alreadyIgnoringBatteryOptim)
            return;

        // Show dialog in UI thread
        var dispatcher = MauiProgram.ServiceProvider.GetRequiredService<IDispatcher>();

        await dispatcher.DispatchAsync(async () =>
        {
            bool openSettings = await Application.Current.Windows[0].Page.DisplayAlertAsync(
                title: "Akku-Optimierung",
                message: BATTERY_OPTIMIZATION_WARNING_TEXT,
                accept: "Einstellungen",
                cancel: "Abbrechen");

            if (!openSettings)
                return;

            var intent = new global::Android.Content.Intent(
                global::Android.Provider.Settings.ActionIgnoreBatteryOptimizationSettings);

            intent.SetFlags(global::Android.Content.ActivityFlags.NewTask);
            context.StartActivity(intent);
        });
    }
}
