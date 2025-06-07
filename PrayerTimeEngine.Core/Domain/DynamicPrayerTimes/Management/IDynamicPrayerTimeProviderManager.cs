using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management;

public interface IDynamicPrayerTimeProviderManager
{
    public Task<DynamicPrayerTimesDaySet> CalculatePrayerTimesAsync(int profileID, ZonedDateTime zoneDate, CancellationToken cancellationToken);
}
