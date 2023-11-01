using PrayerTimeEngine.Core.Common.Enum;
using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.Configuration.Models
{
    public class ProfileTimeConfig
    {
        [Key]
        public int ID { get; set; }

        public int ProfileID { get; set; }
        public Profile Profile { get; set; }
        public ETimeType TimeType { get; set; }
        public GenericSettingConfiguration CalculationConfiguration { get; set; }
    }
}
