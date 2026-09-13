using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models.PrayerTimes;

public class AsrPrayerTime : GenericPrayerTime
{
    public ZonedDateTime? Mithlayn { get; set; }
    public ZonedDateTime? Karaha { get; set; }
}
