using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PrayerTimeEngine.Code.Common.Attribute;
using PrayerTimeEngine.Code.Common.Enum;

namespace PrayerTimeEngine.Code.Domain
{
    public class TimeTypeAttributeService
    {
        public IDictionary<ETimeType, IReadOnlyList<ECalculationSource>> TimeTypeCompatibleSources { get; }
        public List<ETimeType> DegreeTypes { get; }
        public List<ETimeType> SimpleTypes { get; }
        public List<ETimeType> NonSimpleTypes { get; }
        public List<ETimeType> NotHideableTypes { get; }
        public IDictionary<EPrayerType, List<ETimeType>> PrayerTypeToTimeTypes { get; }

        public TimeTypeAttributeService()
        {
            TimeTypeCompatibleSources = new Dictionary<ETimeType, IReadOnlyList<ECalculationSource>>();
            DegreeTypes = new List<ETimeType>();
            SimpleTypes = new List<ETimeType>();
            NonSimpleTypes = new List<ETimeType>();
            NotHideableTypes = new List<ETimeType>();
            PrayerTypeToTimeTypes = new Dictionary<EPrayerType, List<ETimeType>>();
            Initialize();
        }

        private void Initialize()
        {
            foreach (ETimeType type in Enum.GetValues(typeof(ETimeType)))
            {
                var enumType = type.GetType();
                var memberInfos = enumType.GetMember(type.ToString());
                var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
                var timeTypeSupportedByAttrs = enumValueMemberInfo.GetCustomAttributes<TimeTypeSupportedByAttribute>(false);
                var degreeTimeTypeAttrs = enumValueMemberInfo.GetCustomAttributes<DegreeTimeTypeAttribute>(false);
                var simpleTimeTypeAttrs = enumValueMemberInfo.GetCustomAttributes<SimpleTimeTypeAttribute>(false);
                var notHideableTypeAttrs = enumValueMemberInfo.GetCustomAttributes<IsNotHidableTimeTypeAttribute>(false);
                var timeTypeForPrayerTypeAttrs = enumValueMemberInfo.GetCustomAttributes<TimeTypeForPrayerTypeAttribute>(false);

                // Populate CalculationSource compatible types
                foreach (var attr in timeTypeSupportedByAttrs)
                {
                    TimeTypeCompatibleSources[type] = attr.CalculationSources;
                }

                // Populate Degree types
                if (degreeTimeTypeAttrs.Any())
                {
                    DegreeTypes.Add(type);
                }

                // Populate Simple types and non-simple types
                if (simpleTimeTypeAttrs.Any())
                {
                    SimpleTypes.Add(type);
                }
                else
                {
                    NonSimpleTypes.Add(type);
                }

                // Populate Start and End types
                if (notHideableTypeAttrs.Any())
                {
                    NotHideableTypes.Add(type);
                }

                // Populate PrayerType to TimeTypes mapping
                foreach (var attr in timeTypeForPrayerTypeAttrs)
                {
                    if (!PrayerTypeToTimeTypes.ContainsKey(attr.PrayerTime))
                    {
                        PrayerTypeToTimeTypes[attr.PrayerTime] = new List<ETimeType>();
                    }

                    PrayerTypeToTimeTypes[attr.PrayerTime].Add(type);
                }
            }
        }
    }
}