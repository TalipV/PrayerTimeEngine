using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.ConfigStore.Models;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models
{
    public class MuwaqqitCalculationConfiguration : BaseCalculationConfiguration
    {
        protected MuwaqqitCalculationConfiguration(ETimeType timeType, int minuteAdjustment, bool isTimeShown = true)
            : base(minuteAdjustment, isTimeShown)
        {
            TimeType = timeType;
        }

        public override ECalculationSource Source => ECalculationSource.Muwaqqit;
    }
}
