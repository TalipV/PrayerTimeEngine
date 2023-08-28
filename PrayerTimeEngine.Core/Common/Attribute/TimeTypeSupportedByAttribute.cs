using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Common.Attribute
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TimeTypeSupportedByAttribute : System.Attribute
    {
        public List<ECalculationSource> CalculationSources { get; private set; }

        public TimeTypeSupportedByAttribute(params ECalculationSource[] sources)
        {
            CalculationSources = sources.ToList();
        }
    }
}
