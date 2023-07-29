using PrayerTimeEngine.Common.Enum;

namespace PrayerTimeEngine.Domain.CalculationService.Interfaces
{
    public interface ICalculationPrayerTimes
    {
        DateTime GetDateTimeForTimeType(ETimeType timeType);
    }
}
