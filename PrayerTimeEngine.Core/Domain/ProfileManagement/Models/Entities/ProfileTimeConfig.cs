using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities
{
    public class ProfileTimeConfig
    {
        [Key]
        public int ID { get; set; }

        public int ProfileID { get; set; }
        public Profile Profile { get; set; }
        public ETimeType TimeType { get; set; }
        public GenericSettingConfiguration CalculationConfiguration { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not ProfileTimeConfig otherTimeConfig)
                return false;

            return ID == otherTimeConfig.ID
                && ProfileID == otherTimeConfig.ProfileID
                && TimeType == otherTimeConfig.TimeType
                && CalculationConfiguration.Equals(otherTimeConfig.CalculationConfiguration);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, ProfileID, TimeType, CalculationConfiguration);
        }
    }
}
