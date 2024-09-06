using Microsoft.Extensions.DependencyInjection;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers.Mawaqit.Services;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers.MyMosq.Services;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders
{
    public class MosquePrayerTimeProviderFactory(
            IServiceProvider serviceProvider
        ) : IMosquePrayerTimeProviderFactory
    {
        public IMosquePrayerTimeProvider GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(EMosquePrayerTimeProviderType source)
        {
            return source switch
            {
                EMosquePrayerTimeProviderType.Mawaqit => serviceProvider.GetService<MawaqitPrayerTimeService>(),
                EMosquePrayerTimeProviderType.MyMosq=> serviceProvider.GetService<MyMosqPrayerTimeService>(),
                EMosquePrayerTimeProviderType.None => throw new ArgumentException(message: $"'{nameof(EMosquePrayerTimeProviderType.None)}' is not a valid calculation source", paramName: nameof(source)),
                _ => throw new NotImplementedException($"No calculator service implemented for source: {source}"),
            };
        }
    }
}
