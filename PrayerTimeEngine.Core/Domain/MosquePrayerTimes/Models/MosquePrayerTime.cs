using NodaTime;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

public class MosquePrayerTime
{
    public LocalTime Start {  get; set; }
    public LocalTime End {  get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not MosquePrayerTime otherMosquePrayerTime)
            return false;

        return object.Equals(Start, otherMosquePrayerTime.Start)
            && object.Equals(End, otherMosquePrayerTime.End);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }
}
