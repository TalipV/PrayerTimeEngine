using NodaTime;
using PrayerTimeEngine.Core.Domain.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management
{
    public interface IDynamicPrayerTimeProviderManager
    {
        public Task<PrayerTimesBundle> CalculatePrayerTimesAsync(int profileID, ZonedDateTime zoneDate, CancellationToken cancellationToken);
    }
}
