using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Models
{
    public class TimezoneInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string DisplayName { get; set; }
        public string Name { get; set; }
        public int UtcOffsetSeconds { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not TimezoneInfo other)
            {
                return false;
            }

            return ID == other.ID &&
                   DisplayName == other.DisplayName &&
                   Name == other.Name &&
                   UtcOffsetSeconds == other.UtcOffsetSeconds;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, DisplayName, Name, UtcOffsetSeconds);
        }
    }
}
