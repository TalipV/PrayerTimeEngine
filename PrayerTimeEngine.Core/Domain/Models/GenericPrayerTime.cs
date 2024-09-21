using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models;

public class GenericPrayerTime
{
    public ZonedDateTime? Start { get; set; }
    public ZonedDateTime? End { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not GenericPrayerTime otherTime)
            return false;

        return this.Start == otherTime.Start
            && this.End == otherTime.End;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }
}
