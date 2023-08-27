using PrayerTimeEngine.Common.Enum;

namespace PrayerTimeEngine.Domain.CalculationService.Interfaces
{
    public interface ICalculationPrayerTimes
    {
        public DateTime Date { get; }

        public DateTime Fajr { get; }
        public DateTime Shuruq { get; }
        public DateTime Dhuhr { get; }
        public DateTime Asr { get; }
        public DateTime Maghrib { get; }
        public DateTime Isha { get; }

        DateTime GetDateTimeForTimeType(ETimeType timeType);
    }
}
