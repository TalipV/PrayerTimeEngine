using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.Interfaces;

namespace PrayerTimeEngine.Code.Services
{
    public interface IPrayerTimeCalculatorFactory
    {
        public IPrayerTimeCalculator GetService(ECalculationSource source);
    }
}
