using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Common.Attribute;

[AttributeUsage(AttributeTargets.Field)]
public class TimeTypeForPrayerTypeAttribute : System.Attribute
{
    public EPrayerType PrayerTime { get; private set; }

    public TimeTypeForPrayerTypeAttribute(EPrayerType prayerTime)
    {
        PrayerTime = prayerTime;
    }
}
