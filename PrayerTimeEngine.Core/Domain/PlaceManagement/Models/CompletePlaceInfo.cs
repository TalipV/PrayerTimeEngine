using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Data.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Models
{
    public class CompletePlaceInfo : BasicPlaceInfo, IInsertedAt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public Instant? InsertInstant { get; set; }

        public required TimezoneInfo TimezoneInfo { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not CompletePlaceInfo other)
            {
                return false;
            }

            if(ID != other.ID || !GeneralUtil.BetterEquals(TimezoneInfo, other.TimezoneInfo))
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
}
