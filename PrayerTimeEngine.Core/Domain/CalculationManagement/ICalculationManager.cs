using NodaTime;
using PrayerTimeEngine.Core.Domain.Models;

namespace PrayerTimeEngine.Core.Domain.CalculationManagement
{
    public interface IDynamicPrayerTimeProviderManager
    {
        public Task<PrayerTimesBundle> CalculatePrayerTimesAsync(int profileID, ZonedDateTime zoneDate, CancellationToken cancellationToken);
    }
}
