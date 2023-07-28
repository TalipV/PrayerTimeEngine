using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.Models;

namespace PrayerTimeEngine.Code.Domain.CalculationService.Interfaces
{
    public interface IPrayerTimeCalculationService
    {
        public Task<PrayerTimesBundle> ExecuteAsync(Profile profile, DateTime dateTime);
    }
}
