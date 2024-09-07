using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models;

public class MuwaqqitCalculationConfiguration : GenericSettingConfiguration
{
    public override EDynamicPrayerTimeProviderType Source => EDynamicPrayerTimeProviderType.Muwaqqit;
}
