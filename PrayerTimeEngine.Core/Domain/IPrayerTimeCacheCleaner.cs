using NodaTime;

namespace PrayerTimeEngine.Core.Domain;

public interface IPrayerTimeCacheCleaner
{
    Task DeleteCacheDataAsync(LocalDate deleteBeforeDate, CancellationToken cancellationToken);
}
