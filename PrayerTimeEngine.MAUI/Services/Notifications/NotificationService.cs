using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Services.Notifications;

namespace PrayerTimeEngine.Services.Notifications;

public class NotificationService(
        ILogger<NotificationService> logger,
        IPrayerTimeSummaryNotificationHandler prayerTimeSummaryNotificationHandler
    )
{
    public async Task StartPrayerTimeSummaryNotification()
    {
        try
        {
            await prayerTimeSummaryNotificationHandler.ExecuteAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, $"Error at {nameof(StartPrayerTimeSummaryNotification)}");
        }
    }

    // more types of notifications might follow
}
