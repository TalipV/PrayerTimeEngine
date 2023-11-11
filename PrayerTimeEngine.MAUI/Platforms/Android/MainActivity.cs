using Android.App;
using Android.Content.PM;
using Android.OS;

namespace PrayerTimeEngine.Platforms.Android;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
    }

    void createNotificationChannels()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            string name = "Prayer Time Notifications";
            string description = "Updates and reminders for upcoming prayer times.";
            string channelId = "prayer_time_channel";

            var channel = new NotificationChannel(channelId, name, NotificationImportance.Default)
            {
                Description = description
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}