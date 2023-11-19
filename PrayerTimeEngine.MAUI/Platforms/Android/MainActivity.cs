using Android.App;
using Android.Content.PM;
using Android.OS;
using PrayerTimeEngine.Services;

namespace PrayerTimeEngine.Platforms.Android;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public static MainActivity Instance = null;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        Instance = this;
        base.OnCreate(savedInstanceState);
        createNotificationChannel();
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