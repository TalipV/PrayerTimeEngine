using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.ConfigStore.Models;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models
{
    public class MuwaqqitCalculationConfiguration : GenericSettingConfiguration
    {
        public override ECalculationSource Source => ECalculationSource.Muwaqqit;
    }
}
