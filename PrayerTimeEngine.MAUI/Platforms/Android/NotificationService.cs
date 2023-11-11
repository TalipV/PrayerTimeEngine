using Android.App;
using Android.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Platforms.Android
{
    public class NotificationService
    {
        public void ShowNotification(string title, string content)
        {
            var context = Microsoft.Maui.Essentials.Platform.CurrentActivity;


            var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;

            // Create the intent to launch your application when the user taps the notification
            Intent intent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName);
            PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intent, 0);

            // Use the same channel ID as defined in MainActivity
            var channelId = "prayer_time_channel";

            // Build the notification
            var notificationBuilder = new Notification.Builder(context, channelId)
                .SetContentTitle(title)
                .SetContentText(content)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true);

            // Notify
            notificationManager.Notify(0, notificationBuilder.Build());
        }

    }
}
