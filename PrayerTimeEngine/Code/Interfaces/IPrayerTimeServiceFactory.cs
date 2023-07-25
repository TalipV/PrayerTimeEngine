using PrayerTimeEngine.Code.Common.Enum;

namespace PrayerTimeEngine.Code.Interfaces
{
    public interface IPrayerTimeCalculatorFactory
    {
        public IPrayerTimeCalculator GetService(ECalculationSource source);
    }
}
