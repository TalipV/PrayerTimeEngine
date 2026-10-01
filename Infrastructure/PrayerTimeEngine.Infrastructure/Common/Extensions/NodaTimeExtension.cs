using NodaTime;
using NodaTime.Text;

namespace PrayerTimeEngine.Core.Common.Extensions;

internal static class NodaTimeExtensions
{
    private static readonly ZonedDateTimePattern s_zonedDateTimePatternForDBColumn = ZonedDateTimePattern.CreateWithInvariantCulture("G", DateTimeZoneProviders.Tzdb);

    internal static string GetStringForDBColumn(this ZonedDateTime zonedDateTime)
    {
        return s_zonedDateTimePatternForDBColumn.Format(zonedDateTime);
    }

    internal static ZonedDateTime GetZonedDateTimeFromDBColumnString(this string zonedDateTimeString)
    {
        return s_zonedDateTimePatternForDBColumn.Parse(zonedDateTimeString).GetValueOrThrow();
    }

    // ISO 8601 ("uuuu-MM-dd"): fixed-width and lexicographically sortable, so the stored
    // string ordering matches chronological ordering and date columns can be filtered/compared
    // server-side. (The previous "d" pattern produced "MM/dd/yyyy", which does NOT sort correctly.)
    private static readonly LocalDatePattern s_localDatePatternForDBColumn = LocalDatePattern.CreateWithInvariantCulture("uuuu-MM-dd");

    internal static string GetStringForDBColumn(this LocalDate localDate)
    {
        return s_localDatePatternForDBColumn.Format(localDate);
    }

    internal static LocalDate GetLocalDateFromDBColumnString(this string localDateString)
    {
        return s_localDatePatternForDBColumn.Parse(localDateString).GetValueOrThrow();
    }

    // ISO 8601 ("uuuu-MM-ddTHH:mm:ss"): fixed-width and lexicographically sortable, human-readable.
    private static readonly LocalDateTimePattern s_localDateTimePatternForDBColumn = LocalDateTimePattern.CreateWithInvariantCulture("uuuu-MM-ddTHH:mm:ss");

    internal static string GetStringForDBColumn(this LocalDateTime localDateTime)
    {
        return s_localDateTimePatternForDBColumn.Format(localDateTime);
    }
    internal static LocalDateTime GetLocalDateTimeFromDBColumnString(this string localDateTimeString)
    {
        return s_localDateTimePatternForDBColumn.Parse(localDateTimeString).GetValueOrThrow();
    }

    internal static string GetStringForDBColumn(this DateTimeZone dateTimeZone) => dateTimeZone.Id;
    internal static DateTimeZone GetDateTimeZoneFromDBColumnString(this string dateTimeZoneString) => DateTimeZoneProviders.Tzdb[dateTimeZoneString];

    private static readonly LocalTimePattern s_localTimePatternForDBColumn = LocalTimePattern.CreateWithInvariantCulture("HH:mm:ss");

    internal static string GetStringForDBColumn(this LocalTime localTime)
    {
        return s_localTimePatternForDBColumn.Format(localTime);
    }

    internal static LocalTime GetLocalTimeFromDBColumnString(this string localTimeString)
    {
        return s_localTimePatternForDBColumn.Parse(localTimeString).GetValueOrThrow();
    }

    private static readonly InstantPattern s_instantPatternForDBColumn = InstantPattern.CreateWithInvariantCulture("g");

    internal static string GetStringForDBColumn(this Instant instant)
    {
        return s_instantPatternForDBColumn.Format(instant);
    }
    internal static Instant GetInstantFromDBColumnString(this string instantString)
    {
        return s_instantPatternForDBColumn.Parse(instantString).GetValueOrThrow();
    }
}
