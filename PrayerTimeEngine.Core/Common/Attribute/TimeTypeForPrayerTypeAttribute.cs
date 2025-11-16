using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Common.Attribute;

[AttributeUsage(AttributeTargets.Field)]
public class TimeTypeForPrayerTypeAttribute(EPrayerType prayerTime) : System.Attribute
{
    public EPrayerType PrayerTime { get; private set; } = prayerTime;
}
