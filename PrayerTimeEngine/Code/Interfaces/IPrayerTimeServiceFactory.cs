using PrayerTimeEngine.Code.Domain.Model;

namespace PrayerTimeEngine.Code.Interfaces
{
    public interface IPrayerTimeCalculatorFactory
    {
        public IPrayerTimeCalculator GetService(ECalculationSource source);
    }
}
