using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Domain.Models;

public interface IDailyPrayerTimes
{
    public LocalDate Date { get; }
    public DateTimeZone TimeZone { get; }

    public LocalDateTime? Fajr { get; }
    public LocalDateTime? Shuruq { get; }
    public LocalDateTime? Dhuhr { get; }
    public LocalDateTime? Asr { get; }
    public LocalDateTime? Maghrib { get; }
    public LocalDateTime? Isha { get; }

    public ZonedDateTime? GetZonedDateTimeForTimeType(ETimeType timeType);
}
