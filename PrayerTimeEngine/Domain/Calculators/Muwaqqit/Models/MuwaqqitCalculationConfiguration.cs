using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.ConfigStore.Models;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models
{
    public class MuwaqqitCalculationConfiguration : GenericSettingConfiguration
    {
        protected MuwaqqitCalculationConfiguration(ETimeType timeType, int minuteAdjustment, bool isTimeShown = true)
            : base(timeType: timeType, minuteAdjustment: minuteAdjustment, isTimeShown: isTimeShown)
        {
        }

        public override ECalculationSource Source => ECalculationSource.Muwaqqit;
    }
}
