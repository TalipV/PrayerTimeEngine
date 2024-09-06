using NodaTime;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Models
{
    public interface IMosquePrayerTimes
    {
        public string ExternalID { get; }
        public Instant? InsertInstant { get; }

        public LocalDate Date { get; }

        public LocalTime Fajr { get; }
        public LocalTime Shuruq { get; }
        public LocalTime Dhuhr { get; }
        public LocalTime Asr { get; }
        public LocalTime Maghrib { get; }
        public LocalTime Isha { get; }

        public LocalTime? Jumuah { get; }
        public LocalTime? Jumuah2 { get; }

        public LocalTime FajrCongregation { get; }
        public LocalTime DhuhrCongregation { get; }
        public LocalTime AsrCongregation { get; }
        public LocalTime MaghribCongregation { get; }
        public LocalTime IshaCongregation { get; }
    }
}