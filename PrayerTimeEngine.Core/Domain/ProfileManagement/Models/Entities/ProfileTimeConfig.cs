using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

public class ProfileTimeConfig : IInsertedAt
{
    [Key]
    public int ID { get; set; }
    public Instant? InsertInstant { get; set; }

    public int ProfileID { get; set; }
    public DynamicProfile Profile { get; set; }
    public ETimeType TimeType { get; set; }
    public GenericSettingConfiguration CalculationConfiguration { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not ProfileTimeConfig otherTimeConfig)
            return false;

        return ID == otherTimeConfig.ID
            && ProfileID == otherTimeConfig.ProfileID
            // && object.Equals(otherLocationConfig.Profile) why not check it here? why check objects of related data in other Equals implementations?
            && TimeType == otherTimeConfig.TimeType
            && CalculationConfiguration.Equals(otherTimeConfig.CalculationConfiguration);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ID, ProfileID, TimeType, CalculationConfiguration);
    }
}
