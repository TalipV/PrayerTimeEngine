using PrayerTimeEngine.Code.Common.Attribute;
using PrayerTimeEngine.Code.Common.Enum;
using System.Collections.Concurrent;
using System.Linq;

namespace PrayerTimeEngine.Code.Common.Extension
{
    public static class CustomEnumExtension
    {
        private static readonly ConcurrentDictionary<(EPrayerTimeEvent, ECalculationSource), bool> _isSupportedByResultCache = new();

        public static bool IsSupportedBy(this EPrayerTimeEvent timeEvent, ECalculationSource source)
        {
            return 
                _isSupportedByResultCache.GetOrAdd(
                    key: (timeEvent, source), 
                    valueFactory: 
                        (keyValue) =>
                        {
                            // Get the enum field.
                            System.Reflection.FieldInfo field = typeof(EPrayerTimeEvent).GetField(keyValue.Item1.ToString());

                            // Get all SupportedBy attributes on the field.
                            object[] attributes = field.GetCustomAttributes(typeof(SupportedByAttribute), false);

                            // Check if any of the attributes match the provided calculation source.
                            foreach (SupportedByAttribute attribute in attributes)
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
