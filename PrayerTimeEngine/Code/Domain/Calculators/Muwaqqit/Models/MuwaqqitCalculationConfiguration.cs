using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Domain.ConfigStore;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;

namespace PrayerTimeEngine.Code.Domain.Calculator.Muwaqqit.Models
{
    public class MuwaqqitCalculationConfiguration : BaseCalculationConfiguration
    {
        protected MuwaqqitCalculationConfiguration(int minuteAdjustment, bool isTimeShown = true)
            : base(minuteAdjustment, isTimeShown)
        {
        }

        public override ECalculationSource Source => ECalculationSource.Muwaqqit;
    }
}
