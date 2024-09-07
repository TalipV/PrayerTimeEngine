using NodaTime;

namespace PrayerTimeEngine.Core.Domain;

public interface IPrayerTimeCacheCleaner
{
    Task DeleteCacheDataAsync(ZonedDateTime deleteBeforeDate, CancellationToken cancellationToken);
}
