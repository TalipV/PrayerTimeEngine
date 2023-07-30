using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.ConfigStore.Models;

namespace PrayerTimeEngine.Domain.Calculators.Semerkand
{
    public interface IPrayerTimeCalculator
    {
        public Task<ILookup<ICalculationPrayerTimes, ETimeType>> GetPrayerTimesAsync(DateTime date, List<BaseCalculationConfiguration> configurations);
        public HashSet<ETimeType> GetUnsupportedTimeTypes();
    }
}