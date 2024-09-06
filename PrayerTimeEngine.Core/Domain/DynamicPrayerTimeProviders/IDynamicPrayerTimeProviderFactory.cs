using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders
{
    public interface IDynamicPrayerTimeProviderFactory
    {
        IDynamicPrayerTimeProvider GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(EDynamicPrayerTimeProviderType source);
    }
}