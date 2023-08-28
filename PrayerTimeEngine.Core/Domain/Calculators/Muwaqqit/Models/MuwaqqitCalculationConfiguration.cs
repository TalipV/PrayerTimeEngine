using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Configuration.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models
{
    public class MuwaqqitCalculationConfiguration : GenericSettingConfiguration
    {
        public override ECalculationSource Source => ECalculationSource.Muwaqqit;
    }
}
