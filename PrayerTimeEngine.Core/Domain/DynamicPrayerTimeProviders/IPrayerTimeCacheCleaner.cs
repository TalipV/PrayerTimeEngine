using NodaTime;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders
{
    public interface IPrayerTimeCacheCleaner
    {
        Task DeleteCacheDataAsync(ZonedDateTime deleteBeforeDate, CancellationToken cancellationToken);
    }
}
