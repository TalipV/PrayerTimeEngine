using PrayerTimeEngine.Common.Enum;

namespace PrayerTimeEngine.Domain.ConfigStore.Models
{
    public class TimeSpecificConfig
    {
        public int ID { get; set; }
        public int ProfileID { get; set; }
        public ETimeType TimeType { get; set; }
        public GenericSettingConfiguration CalculationConfiguration { get; set; }
    }
}
