using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models.PrayerTimes;

public class DuhaPrayerTime : AbstractPrayerTime
{
    public override string Name => "Duha";
    public ZonedDateTime? QuarterOfDay { get; set; }
}
