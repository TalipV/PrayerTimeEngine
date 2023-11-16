using PropertyChanged;

namespace PrayerTimeEngine.Core.Domain.Configuration.Models
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
                this.ID == otherProfile.ID
                && this.SequenceNo == otherProfile.SequenceNo
                && this.Name == otherProfile.Name
                && this.LocationName == otherProfile.LocationName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, SequenceNo, Name, LocationName);
        }
    }
}
