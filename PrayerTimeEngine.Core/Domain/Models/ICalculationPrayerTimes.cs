using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Domain.Models
{
    public interface ICalculationPrayerTimes
    {
        public LocalDate Date { get; }

        public ZonedDateTime Fajr { get; }
        public ZonedDateTime Shuruq { get; }
        public ZonedDateTime Dhuhr { get; }
        public ZonedDateTime Asr { get; }
        public ZonedDateTime Maghrib { get; }
        public ZonedDateTime Isha { get; }

        ZonedDateTime GetZonedDateTimeForTimeType(ETimeType timeType);
    }
}
