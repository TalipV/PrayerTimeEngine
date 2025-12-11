using NodaTime;
using PrayerTimeEngine.Core.Common;
using System.Globalization;

namespace PrayerTimeEngine.Services;

internal class SystemInfoService : ISystemInfoService
{
    public ZonedDateTime GetCurrentZonedDateTime()
    {
        return GetCurrentInstant().InZone(GetSystemTimeZone());
    }

    public Instant GetCurrentInstant()
    {
        return SystemClock.Instance.GetCurrentInstant();
    }

    public CultureInfo GetSystemCulture()
    {
        return CultureInfo.CurrentCulture;
    }

    public DateTimeZone GetSystemTimeZone()
    {
        if (OperatingSystem.IsWindows())
        {
            return DateTimeZoneProviders.Bcl[TimeZoneInfo.Local.Id];
        }

        return DateTimeZoneProviders.Tzdb[TimeZoneInfo.Local.Id];
    }

    public ZonedDateTime? GetInCurrentZone(ZonedDateTime? zonedDateTime)
    {
        if (zonedDateTime == null)
        {
            return null;
        }

        return GetInCurrentZone(zonedDateTime.Value);
    }

    public ZonedDateTime GetInCurrentZone(ZonedDateTime zonedDateTime)
    {
        DateTimeZone zone = GetSystemTimeZone();
        return zonedDateTime.ToInstant().InZone(zone);
    }
}
