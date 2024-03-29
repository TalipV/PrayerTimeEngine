﻿using NodaTime;
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
            if (OperatingSystem.IsWindows())
            {
                return DateTimeZoneProviders.Bcl[TimeZoneInfo.Local.Id];
            }

            return DateTimeZoneProviders.Tzdb[TimeZoneInfo.Local.Id];
        }
    }
}
