using Android.Content;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using PrayerTimeEngine.Presentation.Popups;
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
            Page? rootPage = Application.Current?.Windows.FirstOrDefault()?.Page;
            Page? page = (rootPage as NavigationPage)?.CurrentPage ?? rootPage;

            if (page is null)
            {
                return;
            }

            IPopupResult<bool> popupResult = await page.ShowPopupAsync<bool>(
                new BatteryOptimizationPopup(),
                BatteryOptimizationPopup.CreatePopupOptions());

            if (!popupResult.Result)
            {
                return;
            }

            RequestBatteryOptimizationExemption(context, package);
        });
    }

    /// <summary>
    /// Opens the system dialog that exempts exactly this app with a single tap ("Allow"). Only if a
    /// device does not offer it, falls back to the list of all apps, where the user has to find this
    /// app and pick "Don't optimize" themselves.
    /// </summary>
    private static void RequestBatteryOptimizationExemption(Context context, string? package)
    {
        try
        {
            var requestIntent = new Intent(
                global::Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations,
                global::Android.Net.Uri.Parse($"package:{package}"));
            requestIntent.SetFlags(ActivityFlags.NewTask);
            context.StartActivity(requestIntent);
        }
        catch (ActivityNotFoundException)
        {
            var settingsIntent = new Intent(
                global::Android.Provider.Settings.ActionIgnoreBatteryOptimizationSettings);
            settingsIntent.SetFlags(ActivityFlags.NewTask);
            context.StartActivity(settingsIntent);
        }
    }
}
