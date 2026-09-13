using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

public abstract class Profile : IEntity
{
    public int ID { get; set; }
    public string Name { get; set; }
    public int SequenceNo { get; set; }
    public Instant? InsertInstant { get; set; }

    #region System.Object overrides

    public override bool Equals(object obj)
    {
        if (obj is not Profile otherProfile)
            return false;

        if (ID != otherProfile.ID
            || SequenceNo != otherProfile.SequenceNo
            || Name != otherProfile.Name)
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ID, SequenceNo, Name);
    }

    #endregion System.Object overrides
}
