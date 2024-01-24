using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Domain.Calculators
{
    public interface IPrayerTimeServiceFactory
    {
        IPrayerTimeService GetPrayerTimeCalculatorByCalculationSource(ECalculationSource source);
    }
}