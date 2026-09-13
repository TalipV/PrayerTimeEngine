using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes;

public interface IMosquePrayerTimeProviderFactory
{
    IMosquePrayerTimeProvider GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(EMosquePrayerTimeProviderType source);
}
