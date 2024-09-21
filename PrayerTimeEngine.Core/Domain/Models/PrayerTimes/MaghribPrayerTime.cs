using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models.PrayerTimes;

public class MaghribPrayerTime : GenericPrayerTime
{
    public ZonedDateTime? SufficientTime { get; set; }
    public ZonedDateTime? Ishtibaq { get; set; }
}
