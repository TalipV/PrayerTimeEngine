using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders
{
    public interface IMosquePrayerTimeProviderFactory
    {
        IMosquePrayerTimeProvider GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(EMosquePrayerTimeProviderType source);
    }
}
