using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.IslamicCalendar.Interfaces;
using System.Globalization;

namespace PrayerTimeEngine.Core.Domain.IslamicCalendar.Services;

public class IslamicDateCalculationService(
        ISystemInfoService systemInfoService
    ) : IIslamicDateCalculationService
{
    private static readonly UmAlQuraCalendar UM_AL_QURA_CALENDAR = new();

    private const int RAMADAN_MONTH_NUMBER = 9;
    private const int DHUL_HIJJAH_MONTH_NUMBER = 12;

    public int GetWeeksUntilRamadan()
    {
        return getWeeksUntilMonth(RAMADAN_MONTH_NUMBER);
    }

    public int GetWeeksUntilHajj()
    {
        return getWeeksUntilMonth(DHUL_HIJJAH_MONTH_NUMBER);
    }

    public int getWeeksUntilMonth(int monthNumber)
    {
        DateTime todayGregorian = systemInfoService.GetCurrentZonedDateTime().Date.ToDateTimeUnspecified();

        int hijriYear = UM_AL_QURA_CALENDAR.GetYear(todayGregorian);

        DateTime monthBeginning = UM_AL_QURA_CALENDAR.ToDateTime(hijriYear, monthNumber, 1, 0, 0, 0, 0);

        if (monthBeginning < todayGregorian)
            monthBeginning = UM_AL_QURA_CALENDAR.ToDateTime(hijriYear + 1, monthNumber, 1, 0, 0, 0, 0);

        double daysUntilMonth = (monthBeginning - todayGregorian).TotalDays;
        return (int)Math.Round(daysUntilMonth / 7, 0, MidpointRounding.AwayFromZero);
    }
}