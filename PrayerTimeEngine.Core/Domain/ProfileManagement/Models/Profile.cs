using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PropertyChanged;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Profile
    {
        public FaziletCountry Country { get; set; } = null;

        public int ID { get; set; }
        public string Name { get; set; }
        public string LocationName { get; set; }
        public int SequenceNo { get; set; }

        public ICollection<ProfileTimeConfig> TimeConfigs { get; set; }
        public ICollection<ProfileLocationConfig> LocationConfigs { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is not Profile otherProfile)
                return false;

            if (ID != otherProfile.ID
                || SequenceNo != otherProfile.SequenceNo
                || Name != otherProfile.Name
                || LocationName != otherProfile.LocationName)
            {
                return false;
            }

            // compare TimeConfigs irrespective of order
            var otherTimeConfigs = otherProfile.TimeConfigs.ToList();
            foreach (ProfileTimeConfig timeConfig in this.TimeConfigs)
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
            foreach (var locationConfig in this.LocationConfigs)
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
            int mainHash = HashCode.Combine(ID, SequenceNo, Name, LocationName);

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
}
