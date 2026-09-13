namespace PrayerTimeEngine.Services.Notifications;

public class NoOpPrayerTimeSummaryNotificationHandler : IPrayerTimeSummaryNotificationHandler
{
    public Task ExecuteAsync() => Task.CompletedTask;
}
