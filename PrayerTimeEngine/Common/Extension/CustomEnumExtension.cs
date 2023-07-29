using PrayerTimeEngine.Common.Attribute;
using PrayerTimeEngine.Common.Enum;
using System.Collections.Concurrent;
using System.Linq;

namespace PrayerTimeEngine.Common.Extension
{
    public static class CustomEnumExtension
    {
        private static readonly ConcurrentDictionary<(ETimeType, ECalculationSource), bool> _isSupportedByResultCache = new();

        public static bool IsSupportedBy(this ETimeType timeType, ECalculationSource source)
        {
            return _isSupportedByResultCache
                .GetOrAdd(
                    key: (timeType, source),
                    valueFactory:
                        (keyValue) =>
                        {
                            // Get the enum field.
                            System.Reflection.FieldInfo field = typeof(ETimeType).GetField(keyValue.Item1.ToString());

                            // Get all SupportedBy attributes on the field.
                            object[] attributes = field.GetCustomAttributes(typeof(TimeTypeSupportedByAttribute), false);

                            // Check if any of the attributes match the provided calculation source.
                            foreach (TimeTypeSupportedByAttribute attribute in attributes)
                            {
                                if (attribute.CalculationSources.Contains(keyValue.Item2))
                                {
                                    return true;
                                }
                            }

                            return false;
                        });
        }
    }
}
