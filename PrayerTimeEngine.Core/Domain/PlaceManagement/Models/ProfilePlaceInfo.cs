using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

public class ProfilePlaceInfo : BasicPlaceInfo, IInsertedAt
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }
    public Instant? InsertInstant { get; set; }
    public int ProfileID { get; set; }
    public DynamicProfile Profile { get; set; }

    public required TimezoneInfo TimezoneInfo { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not ProfilePlaceInfo other)
        {
            return false;
        }

        if (ID != other.ID || ProfileID != other.ProfileID || !Equals(TimezoneInfo, other.TimezoneInfo))
        {
            return false;
        }

        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), ID, TimezoneInfo);
    }
}
