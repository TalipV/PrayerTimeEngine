using Android.App;
using Android.Content;
using PrayerTimeEngine.Platforms.Android.Notifications;

namespace PrayerTimeEngine.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = true, Label = "Boot Receiver")]
[IntentFilter([Intent.ActionBootCompleted])]
public class BootBroadcastReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        if (intent?.Action != Intent.ActionBootCompleted)
            return;

        var startIntent =
            new global::Android.Content.Intent(
                global::Android.App.Application.Context,
                typeof(PrayerTimeSummaryNotification));

        context.StartForegroundService(startIntent);
    }
}
