using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models.PrayerTimes
{
    public class FajrPrayerTime : AbstractPrayerTime
    {
        public override string Name => "Fajr";
        public ZonedDateTime? Ghalas { get; set; }
        public ZonedDateTime? Karaha { get; set; }
    }
}
