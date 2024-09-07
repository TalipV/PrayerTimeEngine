namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

public class MosqueProfile : Profile
{
    public string ExternalID { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not MosqueProfile otherProfile)
            return false;

        if (this.ExternalID != otherProfile.ExternalID)
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), this.ExternalID);
    }
}
