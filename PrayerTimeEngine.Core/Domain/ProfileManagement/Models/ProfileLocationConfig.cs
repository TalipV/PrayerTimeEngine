using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Models
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
                ID == otherLocationConfig.ID
                && ProfileID == otherLocationConfig.ProfileID
                && CalculationSource == otherLocationConfig.CalculationSource
                && LocationData.Equals(otherLocationConfig.LocationData);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, ProfileID, CalculationSource, LocationData);
        }
    }
}
