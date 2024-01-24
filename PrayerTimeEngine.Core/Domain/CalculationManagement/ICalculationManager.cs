using NodaTime;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;

namespace PrayerTimeEngine.Core.Domain.CalculationManagement
{
    public interface ICalculationManager
    {
        public Task<PrayerTimesBundle> CalculatePrayerTimesAsync(Profile profile, ZonedDateTime zoneDate);
    }
}
