using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Core.Domain.CalculationService.Interfaces
{
    public interface IPrayerTimeCalculationService
    {
        public Task<PrayerTimesBundle> ExecuteAsync(Profile profile, DateTime dateTime);
        public IPrayerTimeService GetPrayerTimeCalculatorByCalculationSource(ECalculationSource source);
    }
}
