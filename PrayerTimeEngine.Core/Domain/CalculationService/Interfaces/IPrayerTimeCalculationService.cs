using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Calculators.Semerkand;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.Model;

namespace PrayerTimeEngine.Domain.CalculationService.Interfaces
{
    public interface IPrayerTimeCalculationService
    {
        public Task<PrayerTimesBundle> ExecuteAsync(Profile profile, DateTime dateTime);
        public IPrayerTimeService GetPrayerTimeCalculatorByCalculationSource(ECalculationSource source);
    }
}
