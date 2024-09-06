using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models
{
    public abstract class AbstractPrayerTime
    {
        public abstract string Name { get; }
        public ZonedDateTime? Start { get; set; }
        public ZonedDateTime? End { get; set; }

        public string DurationDisplayText
        {
            get
            {
                string startTime = Start?.ToString("HH:mm:ss", null) ?? "xx:xx:xx";
                string endTime = End?.ToString("HH:mm:ss", null) ?? "xx:xx:xx";

                return $"{startTime} - {endTime}"; ;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is not AbstractPrayerTime otherTime)
                return false;

            return this.Name == otherTime.Name
                && this.Start == otherTime.Start
                && this.End == otherTime.End;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Start, End);
        }
    }
}
