using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Common.Attribute;

[AttributeUsage(AttributeTargets.Field)]
public class TimeTypeForSectionAttribute(ETimeSection section) : System.Attribute
{
    public ETimeSection Section { get; private set; } = section;
}
