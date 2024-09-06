using Microsoft.Extensions.DependencyInjection;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Services;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes
{
    public class MosquePrayerTimeProviderFactory(
            IServiceProvider serviceProvider
        ) : IMosquePrayerTimeProviderFactory
    {
        public IMosquePrayerTimeProvider GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(EMosquePrayerTimeProviderType source)
        {
            return source switch
            {
                EMosquePrayerTimeProviderType.Mawaqit => serviceProvider.GetService<MawaqitMosquePrayerTimeProvider>(),
                EMosquePrayerTimeProviderType.MyMosq => serviceProvider.GetService<MyMosqMosquePrayerTimeProvider>(),
                EMosquePrayerTimeProviderType.None => throw new ArgumentException(message: $"'{nameof(EMosquePrayerTimeProviderType.None)}' is not a valid calculation source", paramName: nameof(source)),
                _ => throw new NotImplementedException($"No calculator service implemented for source: {source}"),
            };
        }
    }
}
