using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace PrayerTimeEngine.Platforms.Android;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        createNotificationChannel();

        Intent startIntent = new Intent(this, typeof(PrayerTimeSummaryNotification));
        StartForegroundService(startIntent);
    }

    void createNotificationChannel()
    {
        string name = "Prayer Time Notifications";
        string description = "Updates and reminders for upcoming prayer times.";

        var channel = new NotificationChannel(PrayerTimeSummaryNotification.CHANNEL_ID, name, NotificationImportance.Default);
        channel.Description = description;
        channel.SetSound(null, null);
        channel.EnableVibration(false);

        var notificationManager = (NotificationManager) GetSystemService(NotificationService);
        notificationManager.CreateNotificationChannel(channel);
    }
}