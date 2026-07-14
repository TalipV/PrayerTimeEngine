using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Domain.Models;

public interface IDailyPrayerTimes
{
    public LocalDate Date { get; }
    public DateTimeZone TimeZone { get; }

    public Instant? Fajr { get; }
    public Instant? Shuruq { get; }
    public Instant? Dhuhr { get; }
    public Instant? Asr { get; }
    public Instant? Maghrib { get; }
    public Instant? Isha { get; }

    public ZonedDateTime? GetZonedDateTimeForTimeType(ETimeType timeType);
}
