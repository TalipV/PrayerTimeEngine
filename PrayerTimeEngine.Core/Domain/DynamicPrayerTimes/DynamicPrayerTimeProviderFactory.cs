using Microsoft.Extensions.DependencyInjection;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;

public class DynamicPrayerTimeProviderFactory(IServiceProvider serviceProvider) : IDynamicPrayerTimeProviderFactory
{
    public IDynamicPrayerTimeProvider GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(EDynamicPrayerTimeProviderType source)
    {
        return source switch
        {
            EDynamicPrayerTimeProviderType.Fazilet => serviceProvider.GetService<FaziletDynamicPrayerTimeProvider>(),
            EDynamicPrayerTimeProviderType.Semerkand => serviceProvider.GetService<SemerkandDynamicPrayerTimeProvider>(),
            EDynamicPrayerTimeProviderType.Muwaqqit => serviceProvider.GetService<MuwaqqitDynamicPrayerTimeProvider>(),
            EDynamicPrayerTimeProviderType.None => throw new ArgumentException(message: $"'{nameof(EDynamicPrayerTimeProviderType.None)}' is not a valid calculation source", paramName: nameof(source)),
            _ => throw new NotImplementedException($"No calculator service implemented for source: {source}"),
        };
    }
}
