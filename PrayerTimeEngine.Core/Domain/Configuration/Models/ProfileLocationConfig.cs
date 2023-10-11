using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Model;
using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.Configuration.Models
{
    public class ProfileLocationConfig
    {
        [Key]
        public int ID { get; set; }

        public int ProfileID { get; set; }
        public Profile Profile { get; set; }
        public ECalculationSource CalculationSource { get; set; }
        public BaseLocationData LocationData { get; set; }
    }
}
