using PrayerTimeEngine.Code.Common.Enum;

namespace PrayerTimeEngine.Code.Domain.CalculationService.Interfaces
{
    public interface ICalculationPrayerTimes
    {
        DateTime GetDateTimeForTimeType(ETimeType timeType);
    }
}
