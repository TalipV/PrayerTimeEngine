using NodaTime;
using System.Globalization;

namespace PrayerTimeEngine.Services.SystemInfoService
{
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
#if WINDOWS
            return DateTimeZoneProviders.Bcl[TimeZoneInfo.Local.Id];
#endif
            return DateTimeZoneProviders.Tzdb[TimeZoneInfo.Local.Id];
        }
    }
}
