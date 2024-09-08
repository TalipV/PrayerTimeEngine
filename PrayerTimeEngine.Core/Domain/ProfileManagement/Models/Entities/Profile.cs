using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PropertyChanged;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

[AddINotifyPropertyChangedInterface]
public abstract class Profile : IInsertedAt
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }
    public string Name { get; set; }
    public int SequenceNo { get; set; }
    public Instant? InsertInstant { get; set; }

    public abstract string GetDisplayText();

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
}
