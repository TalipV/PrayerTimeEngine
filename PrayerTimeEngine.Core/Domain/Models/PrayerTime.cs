using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models
{
    public abstract class PrayerTime
    {
        public abstract string Name { get; }
        public ZonedDateTime? Start { get; set; }
        public ZonedDateTime? End { get; set; }

        public Duration? Duration
        {
            get
            {
                if (Start is null || End is null)
                    return null;

                return Start.Value.ToInstant() - End.Value.ToInstant();
            }
        }

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
            if (obj is not PrayerTime otherTime)
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

    public class FajrPrayerTime : PrayerTime
    {
        public override string Name => "Fajr";
        public ZonedDateTime? Ghalas { get; set; }
        public ZonedDateTime? Karaha { get; set; }
    }

    public class DuhaPrayerTime : PrayerTime
    {
        public override string Name => "Duha";
        public ZonedDateTime? QuarterOfDay { get; set; }
    }

    public class DhuhrPrayerTime : PrayerTime
    {
        public override string Name => "Dhuhr";
    }

    public class AsrPrayerTime : PrayerTime
    {
        public override string Name => "Asr";
        public ZonedDateTime? Mithlayn { get; set; }
        public ZonedDateTime? Karaha { get; set; }
    }

    public class MaghribPrayerTime : PrayerTime
    {
        public override string Name => "Maghrib";
        public ZonedDateTime? SufficientTime { get; set; }
        public ZonedDateTime? Ishtibaq { get; set; }
    }

    public class IshaPrayerTime : PrayerTime
    {
        public override string Name => "Isha";
        public ZonedDateTime? FirstThirdOfNight { get; set; }
        public ZonedDateTime? MiddleOfNight { get; set; }
        public ZonedDateTime? SecondThirdOfNight { get; set; }
    }
}
