using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Data.EntityFramework;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Models
{
    public class CompletePlaceInfo : BasicPlaceInfo, IInsertedAt
    {
        public required TimezoneInfo TimezoneInfo { get; set; }
        public Instant? InsertInstant { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not CompletePlaceInfo other)
            {
                return false;
            }

            if(!GeneralUtil.BetterEquals(TimezoneInfo, other.TimezoneInfo))
            {
                return false;
            }

            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), TimezoneInfo);
        }
    }
}
