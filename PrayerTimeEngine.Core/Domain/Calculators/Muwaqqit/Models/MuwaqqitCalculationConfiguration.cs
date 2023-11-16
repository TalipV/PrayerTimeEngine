using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models
{
    public class MuwaqqitCalculationConfiguration : GenericSettingConfiguration
    {
        public override ECalculationSource Source => ECalculationSource.Muwaqqit;
    }
}
