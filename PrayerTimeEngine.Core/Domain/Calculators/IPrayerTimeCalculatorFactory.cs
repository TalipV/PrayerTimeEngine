using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Domain.Calculators
{
    public interface IDynamicPrayerTimeProviderFactory
    {
        IDynamicPrayerTimeProvider GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(EDynamicPrayerTimeProviderType source);
    }
}