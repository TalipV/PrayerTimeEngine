using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

public class MosqueProfile : Profile
{
    public EMosquePrayerTimeProviderType MosqueProviderType { get; set; } = EMosquePrayerTimeProviderType.None;
    public string ExternalID { get; set; }

    #region System.Object overrides

    public override bool Equals(object obj)
    {
        if (obj is not MosqueProfile otherProfile)
        {
            return false;
        }

        if (ExternalID != otherProfile.ExternalID
            || MosqueProviderType != otherProfile.MosqueProviderType)
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), ExternalID, MosqueProviderType);
    }

    #endregion System.Object overrides
}
