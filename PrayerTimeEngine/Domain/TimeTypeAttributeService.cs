using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PrayerTimeEngine.Common.Attribute;
using PrayerTimeEngine.Common.Enum;

namespace PrayerTimeEngine.Domain
{
    public class TimeTypeAttributeService
    {
        public IDictionary<ETimeType, IReadOnlyList<ECalculationSource>> TimeTypeCompatibleSources { get; }
        public List<ETimeType> DegreeTypes { get; }
        public List<ETimeType> SimpleTypes { get; }
        public List<ETimeType> NonSimpleTypes { get; }
        public List<ETimeType> NotHideableTypes { get; }
        public List<ETimeType> ConfigurableSimpleTypes { get; }
        public List<ETimeType> ConfigurableTypes { get; }
        public IDictionary<EPrayerType, List<ETimeType>> PrayerTypeToTimeTypes { get; }

        public TimeTypeAttributeService()
        {
            TimeTypeCompatibleSources = new Dictionary<ETimeType, IReadOnlyList<ECalculationSource>>();
            DegreeTypes = new List<ETimeType>();
            SimpleTypes = new List<ETimeType>();
            NonSimpleTypes = new List<ETimeType>();
            NotHideableTypes = new List<ETimeType>();
            ConfigurableSimpleTypes = new List<ETimeType>();
            ConfigurableTypes = new List<ETimeType>();
            PrayerTypeToTimeTypes = new Dictionary<EPrayerType, List<ETimeType>>();
            Initialize();
        }

        private void Initialize()
        {
            Type enumType = typeof(ETimeType);

            foreach (ETimeType timeType in Enum.GetValues(typeof(ETimeType)))
            {
                MemberInfo[] memberInfos = enumType.GetMember(timeType.ToString());
                MemberInfo enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);

                List<TimeTypeSupportedByAttribute> timeTypeSupportedByAttrs = enumValueMemberInfo.GetCustomAttributes<TimeTypeSupportedByAttribute>(false).ToList();
                List<DegreeTimeTypeAttribute> degreeTimeTypeAttrs = enumValueMemberInfo.GetCustomAttributes<DegreeTimeTypeAttribute>(false).ToList();
                List<ConfigurableSimpleTypeAttribute> configurableSimpleTimeTypeAttrs = enumValueMemberInfo.GetCustomAttributes<ConfigurableSimpleTypeAttribute>(false).ToList();
                List<SimpleTimeTypeAttribute> simpleTimeTypeAttrs = enumValueMemberInfo.GetCustomAttributes<SimpleTimeTypeAttribute>(false).ToList();
                List<IsNotHidableTimeTypeAttribute> notHideableTypeAttrs = enumValueMemberInfo.GetCustomAttributes<IsNotHidableTimeTypeAttribute>(false).ToList();
                List<TimeTypeForPrayerTypeAttribute> timeTypeForPrayerTypeAttrs = enumValueMemberInfo.GetCustomAttributes<TimeTypeForPrayerTypeAttribute>(false).ToList();

                foreach (TimeTypeSupportedByAttribute attr in timeTypeSupportedByAttrs)
                {
                    TimeTypeCompatibleSources[timeType] = attr.CalculationSources;
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
                    NonSimpleTypes.Add(timeType);
                    ConfigurableTypes.Add(timeType);
                }

                if (notHideableTypeAttrs.Count != 0)
                {
                    NotHideableTypes.Add(timeType);
                }

                foreach (var attr in timeTypeForPrayerTypeAttrs)
                {
                    if (!PrayerTypeToTimeTypes.ContainsKey(attr.PrayerTime))
                    {
                        PrayerTypeToTimeTypes[attr.PrayerTime] = new List<ETimeType>();
                    }

                    PrayerTypeToTimeTypes[attr.PrayerTime].Add(timeType);
                }
            }
        }
    }
}