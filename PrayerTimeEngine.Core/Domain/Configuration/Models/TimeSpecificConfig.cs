using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Domain.Configuration.Models
{
    public class TimeSpecificConfig
    {
        public int ID { get; set; }
        public int ProfileID { get; set; }
        public ETimeType TimeType { get; set; }
        public GenericSettingConfiguration CalculationConfiguration { get; set; }
    }
}
