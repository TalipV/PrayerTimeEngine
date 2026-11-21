using NodaTime;

namespace PrayerTimeEngine.Core.Domain;

public interface IPrayerTimeCacheCleaner
{
    /// <summary>
    /// Deletes the cached prayer times that were stored before <paramref name="insertedBefore"/>, no matter
    /// which day they belong to. Cached days of the past (e.g. looked at in the app) thereby stay as long as
    /// the days of the future, and refreshed data replaces old data after the same time.
    /// </summary>
    Task DeleteCacheDataAsync(Instant insertedBefore, CancellationToken cancellationToken);
}
