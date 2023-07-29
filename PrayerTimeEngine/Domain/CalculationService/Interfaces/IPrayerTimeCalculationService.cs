using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.Model;

namespace PrayerTimeEngine.Domain.CalculationService.Interfaces
{
    public interface IPrayerTimeCalculationService
    {
        public Task<PrayerTimesBundle> ExecuteAsync(Profile profile, DateTime dateTime);
    }
}
