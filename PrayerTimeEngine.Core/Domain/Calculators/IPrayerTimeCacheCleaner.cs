using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Calculators
{
    public interface IPrayerTimeCacheCleaner
    {
        Task DeleteCacheDataAsync(ZonedDateTime deleteBeforeDate, CancellationToken cancellationToken);
    }
}
