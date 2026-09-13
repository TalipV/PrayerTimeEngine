using NodaTime;
using NodaTime.Text;

namespace PrayerTimeEngine.Core.Common.Extensions;

internal static class NodaTimeExtensions
{
    private static readonly ZonedDateTimePattern _zonedDateTimePatternForDBColumn = ZonedDateTimePattern.CreateWithInvariantCulture("G", DateTimeZoneProviders.Tzdb);

    internal static string GetStringForDBColumn(this ZonedDateTime zonedDateTime)
    {
        return _zonedDateTimePatternForDBColumn.Format(zonedDateTime);
    }

    internal static ZonedDateTime GetZonedDateTimeFromDBColumnString(this string zonedDateTimeString)
    {
        return _zonedDateTimePatternForDBColumn.Parse(zonedDateTimeString).GetValueOrThrow();
    }

    // ISO 8601 ("uuuu-MM-dd"): fixed-width and lexicographically sortable, so the stored
    // string ordering matches chronological ordering and date columns can be filtered/compared
    // server-side. (The previous "d" pattern produced "MM/dd/yyyy", which does NOT sort correctly.)
    private static readonly LocalDatePattern _localDatePatternForDBColumn = LocalDatePattern.CreateWithInvariantCulture("uuuu-MM-dd");

    internal static string GetStringForDBColumn(this LocalDate localDate)
    {
        return _localDatePatternForDBColumn.Format(localDate);
    }

    internal static LocalDate GetLocalDateFromDBColumnString(this string localDateString)
    {
        return _localDatePatternForDBColumn.Parse(localDateString).GetValueOrThrow();
    }

    // ISO 8601 ("uuuu-MM-ddTHH:mm:ss"): fixed-width and lexicographically sortable, human-readable.
    private static readonly LocalDateTimePattern _localDateTimePatternForDBColumn = LocalDateTimePattern.CreateWithInvariantCulture("uuuu-MM-ddTHH:mm:ss");

    internal static string GetStringForDBColumn(this LocalDateTime localDateTime)
    {
        return _localDateTimePatternForDBColumn.Format(localDateTime);
    }
    internal static LocalDateTime GetLocalDateTimeFromDBColumnString(this string localDateTimeString)
    {
        return _localDateTimePatternForDBColumn.Parse(localDateTimeString).GetValueOrThrow();
    }

    internal static string GetStringForDBColumn(this DateTimeZone dateTimeZone) => dateTimeZone.Id;
    internal static DateTimeZone GetDateTimeZoneFromDBColumnString(this string dateTimeZoneString) => DateTimeZoneProviders.Tzdb[dateTimeZoneString];

    private static readonly LocalTimePattern _localTimePatternForDBColumn = LocalTimePattern.CreateWithInvariantCulture("HH:mm:ss");

    internal static string GetStringForDBColumn(this LocalTime LocalTime)
    {
        return _localTimePatternForDBColumn.Format(LocalTime);
    }

    internal static LocalTime GetLocalTimeFromDBColumnString(this string LocalTimeString)
    {
        return _localTimePatternForDBColumn.Parse(LocalTimeString).GetValueOrThrow();
    }

    private static readonly InstantPattern _instantPatternForDBColumn = InstantPattern.CreateWithInvariantCulture("g");

    internal static string GetStringForDBColumn(this Instant instant)
    {
        return _instantPatternForDBColumn.Format(instant);
    }
    internal static Instant GetInstantFromDBColumnString(this string instantString)
    {
        return _instantPatternForDBColumn.Parse(instantString).GetValueOrThrow();
    }
}
