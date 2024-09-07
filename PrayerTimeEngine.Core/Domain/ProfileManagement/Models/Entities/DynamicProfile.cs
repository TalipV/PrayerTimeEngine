using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

public class DynamicProfile : Profile
{
    public ProfilePlaceInfo PlaceInfo { get; set; }
    public ICollection<ProfileTimeConfig> TimeConfigs { get; set; }
    public ICollection<ProfileLocationConfig> LocationConfigs { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not DynamicProfile otherProfile)
            return false;

        if (!base.Equals(otherProfile))
        {
            return false;
        }

        if (!object.Equals(PlaceInfo, otherProfile.PlaceInfo))
        {
            return false;
        }

        // compare TimeConfigs irrespective of order
        var otherTimeConfigs = otherProfile.TimeConfigs.ToList();
        foreach (ProfileTimeConfig timeConfig in TimeConfigs)
        {
            if (otherTimeConfigs.FirstOrDefault(x => x.Equals(timeConfig)) is ProfileTimeConfig match)
                otherTimeConfigs.Remove(match);
            else
                return false;
        }
        if (otherTimeConfigs.Count != 0)    // not possible but.. why not
            return false;

        // compare LocationConfigs irrespective of order
        var otherLocationConfigs = otherProfile.LocationConfigs.ToList();
        foreach (var locationConfig in LocationConfigs)
        {
            if (otherLocationConfigs.FirstOrDefault(x => x.Equals(locationConfig)) is ProfileLocationConfig match)
                otherLocationConfigs.Remove(match);
            else
                return false;
        }
        if (otherLocationConfigs.Count != 0)    // not possible but.. why not
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        int mainHash = HashCode.Combine(base.GetHashCode(), PlaceInfo);

        // Aggregate hash codes of TimeConfigs
        int timeConfigsHash = 0;
        foreach (var timeConfig in TimeConfigs)
        {
            // Use unchecked to ignore overflow, as overflow is fine in hash code calculations
            unchecked
            {
                timeConfigsHash += timeConfig.GetHashCode();
            }
        }

        // Aggregate hash codes of LocationConfigs
        int locationConfigsHash = 0;
        foreach (var locationConfig in LocationConfigs)
        {
            // Use unchecked to ignore overflow
            unchecked
            {
                locationConfigsHash += locationConfig.GetHashCode();
            }
        }
        // ^ by ChatGPT

        // Combine aggregated hash codes with the existing hash
        return HashCode.Combine(mainHash, timeConfigsHash, locationConfigsHash);
    }
}
