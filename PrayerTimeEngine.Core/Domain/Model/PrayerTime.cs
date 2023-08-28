namespace PrayerTimeEngine.Core.Domain.Model
{
    public abstract class PrayerTime
    {
        public abstract string Name { get; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public string DurationDisplayText
        {
            get
            {
                string startTime = Start?.ToString("HH:mm:ss") ?? "xx:xx:xx";
                string endTime = End?.ToString("HH:mm:ss") ?? "xx:xx:xx";

                return $"{startTime} - {endTime}"; ;
            }
        }
    }

    public class FajrPrayerTime : PrayerTime
    {
        public override string Name => "Fajr";
        public DateTime? Ghalas { get; set; }
        public DateTime? Karaha { get; set; }
    }

    public class DuhaPrayerTime : PrayerTime
    {
        public override string Name => "Duha";
        public DateTime? QuarterOfDay { get; set; }
    }

    public class DhuhrPrayerTime : PrayerTime
    {
        public override string Name => "Dhuhr";
    }

    public class AsrPrayerTime : PrayerTime
    {
        public override string Name => "Asr";
        public DateTime? Mithlayn { get; set; }
        public DateTime? Karaha { get; set; }
    }

    public class MaghribPrayerTime : PrayerTime
    {
        public override string Name => "Maghrib";
        public DateTime? SufficientTime { get; set; }
        public DateTime? Ishtibaq { get; set; }
    }

    public class IshaPrayerTime : PrayerTime
    {
        public override string Name => "Isha";
        public DateTime? FirstThirdOfNight { get; set; }
        public DateTime? MiddleOfNight { get; set; }
        public DateTime? SecondThirdOfNight { get; set; }
    }
}
