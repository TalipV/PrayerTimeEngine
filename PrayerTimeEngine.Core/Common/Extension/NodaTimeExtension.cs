using NodaTime;
using NodaTime.Text;

namespace PrayerTimeEngine.Core.Common.Extension
{
    internal static class NodaTimeExtensions
    {
        private static readonly InstantPattern _instantPatternForDBColumn = InstantPattern.CreateWithInvariantCulture("g");

        internal static string GetStringForDBColumn(this Instant instant)
        {
            return _instantPatternForDBColumn.Format(instant);
        }
        internal static Instant GetInstantFromDBColumnString(this string instantString)
        {
            return _instantPatternForDBColumn.Parse(instantString).GetValueOrThrow();
        }

        private static readonly ZonedDateTimePattern _zonedDateTimePatternForDBColumn = ZonedDateTimePattern.CreateWithInvariantCulture("G", DateTimeZoneProviders.Tzdb);

        internal static string GetStringForDBColumn(this ZonedDateTime zonedDateTime)
        {
            return _zonedDateTimePatternForDBColumn.Format(zonedDateTime);
        }

        internal static ZonedDateTime GetZonedDateTimeFromDBColumnString(this string zonedDateTimeString)
        {
            return _zonedDateTimePatternForDBColumn.Parse(zonedDateTimeString).GetValueOrThrow();
        }

        private static readonly LocalDatePattern _localDatePatternForDBColumn = LocalDatePattern.CreateWithInvariantCulture("d");

        internal static string GetStringForDBColumn(this LocalDate localDate)
        {
            return _localDatePatternForDBColumn.Format(localDate);
        }

        internal static LocalDate GetLocalDateFromDBColumnString(this string localDateString)
        {
            return _localDatePatternForDBColumn.Parse(localDateString).GetValueOrThrow();
        }
    }
}
