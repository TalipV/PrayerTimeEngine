using NodaTime;
using System.Globalization;

namespace PrayerTimeEngine.Services.SystemInfoService
{
    public interface ISystemInfoService
    {
        ZonedDateTime GetCurrentZonedDateTime();
        Instant GetCurrentInstant();
        DateTimeZone GetSystemTimeZone();
        CultureInfo GetSystemCulture();
    }
}
