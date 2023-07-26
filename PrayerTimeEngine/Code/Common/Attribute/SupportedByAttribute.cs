using PrayerTimeEngine.Code.Common.Enum;

namespace PrayerTimeEngine.Code.Common.Attribute
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SupportedByAttribute : System.Attribute
    { 
        public HashSet<ECalculationSource> CalculationSources { get; private set; }

        public SupportedByAttribute(params ECalculationSource[] sources)
        {
            CalculationSources = sources.ToHashSet();
        }
    }
}
