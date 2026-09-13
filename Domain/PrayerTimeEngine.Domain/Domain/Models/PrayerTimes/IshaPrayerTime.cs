using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models.PrayerTimes;

public class IshaPrayerTime : GenericPrayerTime
{
    public ZonedDateTime? FirstThirdOfNight { get; set; }
    public ZonedDateTime? MiddleOfNight { get; set; }
    public ZonedDateTime? SecondThirdOfNight { get; set; }
}
