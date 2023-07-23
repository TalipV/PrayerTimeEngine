using PrayerTimeEngine.Code.Common.Enums;

namespace PrayerTimeEngine.Code.Interfaces
{
    public interface IPrayerTimeCalculatorFactory
    {
        public IPrayerTimeCalculator GetService(ECalculationSource source);
    }
}
