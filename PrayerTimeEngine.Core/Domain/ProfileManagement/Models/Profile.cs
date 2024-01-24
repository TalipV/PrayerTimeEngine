using PropertyChanged;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Profile
    {
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

            return
                ID == otherProfile.ID
                && SequenceNo == otherProfile.SequenceNo
                && Name == otherProfile.Name
                && LocationName == otherProfile.LocationName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, SequenceNo, Name, LocationName);
        }
    }
}
