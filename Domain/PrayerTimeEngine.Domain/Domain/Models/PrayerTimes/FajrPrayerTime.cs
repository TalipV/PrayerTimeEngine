using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models.PrayerTimes;

public class FajrPrayerTime : GenericPrayerTime
{
    public ZonedDateTime? Ghalas { get; set; }
    public ZonedDateTime? Karaha { get; set; }
}
