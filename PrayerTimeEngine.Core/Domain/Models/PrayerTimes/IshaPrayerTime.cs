using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models.PrayerTimes
{
    public class IshaPrayerTime : AbstractPrayerTime
    {
        public override string Name => "Isha";
        public ZonedDateTime? FirstThirdOfNight { get; set; }
        public ZonedDateTime? MiddleOfNight { get; set; }
        public ZonedDateTime? SecondThirdOfNight { get; set; }
    }
}
