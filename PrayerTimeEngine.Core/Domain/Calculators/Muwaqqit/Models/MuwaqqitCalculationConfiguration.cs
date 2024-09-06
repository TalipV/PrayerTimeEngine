using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models
{
    public class MuwaqqitCalculationConfiguration : GenericSettingConfiguration
    {
        public override EDynamicPrayerTimeProviderType Source => EDynamicPrayerTimeProviderType.Muwaqqit;
    }
}
