using PrayerTimeEngine.Core.Domain.Models;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

public class MosquePrayerTime : GenericPrayerTime
{
    public int CongregationStartOffset { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not MosquePrayerTime otherTime)
            return false;

        return base.Equals(obj)
            && this.CongregationStartOffset == otherTime.CongregationStartOffset;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), CongregationStartOffset);
    }
}
