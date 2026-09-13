using PrayerTimeEngine.Core.Common.Attribute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using System.Reflection;

namespace PrayerTimeEngine.Core.Domain;

public class TimeTypeAttributeService
{
    public IDictionary<ETimeType, IReadOnlyList<EDynamicPrayerTimeProviderType>> TimeTypeCompatibleSources { get; }
    public List<ETimeType> DegreeTypes { get; }
    public List<ETimeType> SimpleTypes { get; }
    public List<ETimeType> ComplexTypes { get; }
    public List<ETimeType> NotHideableTypes { get; }
    public List<ETimeType> ConfigurableSimpleTypes { get; }
    public List<ETimeType> ConfigurableTypes { get; }
    public IDictionary<ETimeSection, List<ETimeType>> SectionToTimeTypes { get; }

    public TimeTypeAttributeService()
    {
        TimeTypeCompatibleSources = new Dictionary<ETimeType, IReadOnlyList<EDynamicPrayerTimeProviderType>>();
        DegreeTypes = [];
        SimpleTypes = [];
        ComplexTypes = [];
        NotHideableTypes = [];
        ConfigurableSimpleTypes = [];
        ConfigurableTypes = [];
        SectionToTimeTypes = new Dictionary<ETimeSection, List<ETimeType>>();
        Initialize();
    }

    private void Initialize()
    {
        Type enumType = typeof(ETimeType);

        foreach (ETimeType timeType in Enum.GetValues<ETimeType>())
        {
            MemberInfo[] memberInfos = enumType.GetMember(timeType.ToString());
            MemberInfo enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);

            List<TimeTypeSupportedByAttribute> timeTypeSupportedByAttrs = enumValueMemberInfo.GetCustomAttributes<TimeTypeSupportedByAttribute>(false).ToList();
            List<DegreeTimeTypeAttribute> degreeTimeTypeAttrs = enumValueMemberInfo.GetCustomAttributes<DegreeTimeTypeAttribute>(false).ToList();
            List<ConfigurableSimpleTypeAttribute> configurableSimpleTimeTypeAttrs = enumValueMemberInfo.GetCustomAttributes<ConfigurableSimpleTypeAttribute>(false).ToList();
            List<SimpleTimeTypeAttribute> simpleTimeTypeAttrs = enumValueMemberInfo.GetCustomAttributes<SimpleTimeTypeAttribute>(false).ToList();
            List<IsNotHidableTimeTypeAttribute> notHideableTypeAttrs = enumValueMemberInfo.GetCustomAttributes<IsNotHidableTimeTypeAttribute>(false).ToList();
            List<TimeTypeForSectionAttribute> timeTypeForSectionAttrs = enumValueMemberInfo.GetCustomAttributes<TimeTypeForSectionAttribute>(false).ToList();

            foreach (TimeTypeSupportedByAttribute attr in timeTypeSupportedByAttrs)
            {
                TimeTypeCompatibleSources[timeType] = attr.DynamicPrayerTimeProviders;
            }

            if (degreeTimeTypeAttrs.Count != 0)
            {
                DegreeTypes.Add(timeType);
            }

            if (simpleTimeTypeAttrs.Count != 0 || configurableSimpleTimeTypeAttrs.Count != 0)
            {
                SimpleTypes.Add(timeType);

                if (configurableSimpleTimeTypeAttrs.Count != 0)
                {
                    ConfigurableTypes.Add(timeType);
                    ConfigurableSimpleTypes.Add(timeType);
                }
            }
            else
            {
                ComplexTypes.Add(timeType);
                ConfigurableTypes.Add(timeType);
            }

            if (notHideableTypeAttrs.Count != 0)
            {
                NotHideableTypes.Add(timeType);
            }

            foreach (var attr in timeTypeForSectionAttrs)
            {
                if (!SectionToTimeTypes.TryGetValue(attr.Section, out List<ETimeType> value))
                {
                    value = [];
                    SectionToTimeTypes[attr.Section] = value;
                }

                value.Add(timeType);
            }
        }
    }
}