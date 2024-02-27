using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Domain.Calculators
{
    public interface IPrayerTimeCalculatorFactory
    {
        IPrayerTimeCalculator GetPrayerTimeCalculatorByCalculationSource(ECalculationSource source);
    }
}