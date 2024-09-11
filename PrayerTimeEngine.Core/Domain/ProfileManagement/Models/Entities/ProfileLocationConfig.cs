using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

public class ProfileLocationConfig : IInsertedAt
{
    [Key]
    public int ID { get; set; }
    public Instant? InsertInstant { get; set; }
    public int ProfileID { get; set; }
    public DynamicProfile Profile { get; set; }
    public EDynamicPrayerTimeProviderType DynamicPrayerTimeProvider { get; set; }
    public BaseLocationData LocationData { get; set; }
    public override bool Equals(object obj)
    {
        if (obj is not ProfileLocationConfig otherLocationConfig)
            return false;

        return ID == otherLocationConfig.ID
            && ProfileID == otherLocationConfig.ProfileID
            // && object.Equals(otherLocationConfig.Profile) why not check it here? why check objects of related data in other Equals implementations?
            && DynamicPrayerTimeProvider == otherLocationConfig.DynamicPrayerTimeProvider
            && LocationData.Equals(otherLocationConfig.LocationData);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ID, ProfileID, DynamicPrayerTimeProvider, LocationData);
    }
}
