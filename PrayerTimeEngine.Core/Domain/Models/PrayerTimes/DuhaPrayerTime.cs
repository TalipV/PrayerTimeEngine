using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models.PrayerTimes;

public class DuhaPrayerTime : GenericPrayerTime
{
    public ZonedDateTime? QuarterOfDay { get; set; }
}
