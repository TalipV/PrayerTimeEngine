using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;

namespace PrayerTimeEngine.Core.Common.Attribute;

[AttributeUsage(AttributeTargets.Field)]
public class TimeTypeSupportedByAttribute : System.Attribute
{
    public List<EDynamicPrayerTimeProviderType> DynamicPrayerTimeProviders { get; private set; }

    public TimeTypeSupportedByAttribute(params EDynamicPrayerTimeProviderType[] sources)
    {
        DynamicPrayerTimeProviders = sources.ToList();
    }
}
