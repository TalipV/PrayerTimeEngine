using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management;

public interface IDynamicPrayerTimeProviderManager
{
    public Task<CalculatePrayerTimesResultVO> CalculatePrayerTimesAsync(int profileID, ZonedDateTime zoneDate, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the already calculated day set for the profile from the in-memory cache without ever
    /// triggering a (potentially networking) calculation. Meant for cheap, frequent consumers like the
    /// persistent notification that only want to render already available times and must not be blocked
    /// or fail on a cold cache.
    /// </summary>
    public bool TryGetCachedPrayerTimes(int profileID, ZonedDateTime zoneDate, out DynamicPrayerTimesDaySet daySet);
}
