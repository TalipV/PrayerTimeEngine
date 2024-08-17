using NodaTime;
using System.Globalization;

namespace PrayerTimeEngine.Core.Common
{
    public interface ISystemInfoService
    {
        ZonedDateTime GetCurrentZonedDateTime();
        Instant GetCurrentInstant();
        DateTimeZone GetSystemTimeZone();
        CultureInfo GetSystemCulture();
    }
}
