using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models
{
    public interface IDailyPrayerTimes
    {
        public ZonedDateTime Date { get; }

        public ZonedDateTime Fajr { get; }
        public ZonedDateTime Shuruq { get; }
        public ZonedDateTime Dhuhr { get; }
        public ZonedDateTime Asr { get; }
        public ZonedDateTime Maghrib { get; }
        public ZonedDateTime Isha { get; }
    }
}
