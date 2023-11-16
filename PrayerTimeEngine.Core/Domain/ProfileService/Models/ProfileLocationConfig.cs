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
        public override bool Equals(object obj)
        {
            if (obj is not ProfileLocationConfig otherLocationConfig)
                return false;

            return
                this.ID == otherLocationConfig.ID
                && this.ProfileID == otherLocationConfig.ProfileID
                && this.CalculationSource == otherLocationConfig.CalculationSource
                && this.LocationData.Equals(otherLocationConfig.LocationData);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, ProfileID, CalculationSource, LocationData);
        }
    }
}
