using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;

namespace PrayerTimeEngine.Code.Domain.Calculators
{
    public interface IPrayerTimeCalculator
    {
        public Task<DateTime> GetPrayerTimesAsync(
            DateTime date,
            ETimeType timeType,
            BaseCalculationConfiguration configuration);

        public HashSet<ETimeType> GetUnsupportedCalculationTimeTypes();
    }
}