using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes
{
    public interface IDynamicPrayerTimeProviderFactory
    {
        IDynamicPrayerTimeProvider GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(EDynamicPrayerTimeProviderType source);
    }
}