using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Core.Domain.CalculationManager
{
    public interface IPrayerTimeCalculationManager
    {
        public Task<PrayerTimesBundle> CalculatePrayerTimesAsync(Profile profile, LocalDate date);
        public IPrayerTimeService GetPrayerTimeCalculatorByCalculationSource(ECalculationSource source);
    }
}
