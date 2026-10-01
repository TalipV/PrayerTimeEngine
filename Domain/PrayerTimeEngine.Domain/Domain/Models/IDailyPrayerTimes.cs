using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Domain.Models;

public interface IDailyPrayerTimes
{
    LocalDate Date { get; }
    DateTimeZone TimeZone { get; }

    Instant? Fajr { get; }
    Instant? Shuruq { get; }
    Instant? Dhuhr { get; }
    Instant? Asr { get; }
    Instant? Maghrib { get; }
    Instant? Isha { get; }

    ZonedDateTime? GetZonedDateTimeForTimeType(ETimeType timeType);
}
