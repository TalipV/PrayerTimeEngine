using PrayerTimeEngine.Code.Common.Enum;

namespace PrayerTimeEngine.Code.Common.Attribute
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
