using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management;

public interface IDynamicPrayerTimeProviderManager
{
    public Task<CalculatePrayerTimesResultVO> CalculatePrayerTimesAsync(int profileID, ZonedDateTime zoneDate, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the day set for the profile only if it is already available, without ever triggering a
    /// (potentially networking) calculation. Meant for cheap, frequent consumers like the persistent
    /// notification that only want to render already calculated times and must not be blocked or fail
    /// when nothing has been calculated yet. Returns <c>false</c> (and a null <paramref name="daySet"/>)
    /// when no result is available; how availability is determined is an implementation detail.
    /// </summary>
    public bool TryGetAlreadyCalculatedPrayerTimes(int profileID, ZonedDateTime zoneDate, out DynamicPrayerTimesDaySet daySet);
}
