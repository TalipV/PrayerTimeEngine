using System.Globalization;
using NodaTime;

namespace PrayerTimeEngine.Core.Common;

public interface ISystemInfoService
{
    ZonedDateTime GetCurrentZonedDateTime();
    Instant GetCurrentInstant();
    DateTimeZone GetSystemTimeZone();
    CultureInfo GetSystemCulture();
    ZonedDateTime? GetInCurrentZone(ZonedDateTime? zonedDateTime);
    ZonedDateTime GetInCurrentZone(ZonedDateTime zonedDateTime);
}
