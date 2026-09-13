using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;

namespace PrayerTimeEngine.Core.Common.Attribute;

[AttributeUsage(AttributeTargets.Field)]
public class TimeTypeSupportedByAttribute(params EDynamicPrayerTimeProviderType[] sources) : System.Attribute
{
    public List<EDynamicPrayerTimeProviderType> DynamicPrayerTimeProviders { get; private set; } = sources.ToList();
}
