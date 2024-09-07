using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models.PrayerTimes;

public class MaghribPrayerTime : AbstractPrayerTime
{
    public override string Name => "Maghrib";
    public ZonedDateTime? SufficientTime { get; set; }
    public ZonedDateTime? Ishtibaq { get; set; }
}
