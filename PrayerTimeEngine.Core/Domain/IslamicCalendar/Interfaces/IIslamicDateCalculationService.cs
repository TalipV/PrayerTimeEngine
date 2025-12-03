namespace PrayerTimeEngine.Core.Domain.IslamicCalendar.Interfaces
{
    public interface IIslamicDateCalculationService
    {
        int GetWeeksUntilRamadan();
        int GetWeeksUntilHajj();
    }
}
