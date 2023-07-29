using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.ConfigStore.Models;

namespace PrayerTimeEngine.Domain.Calculators
{
    public interface IPrayerTimeCalculator
    {
        public Task<ICalculationPrayerTimes> GetPrayerTimesAsync(DateTime date, BaseCalculationConfiguration configuration);
        public HashSet<ETimeType> GetUnsupportedCalculationTimeTypes();
    }
}