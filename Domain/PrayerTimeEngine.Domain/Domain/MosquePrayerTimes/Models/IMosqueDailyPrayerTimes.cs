using NodaTime;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

public interface IMosqueDailyPrayerTimes
{
    string ExternalID { get; }
    Instant? InsertInstant { get; }

    LocalDate Date { get; }

    LocalTime? Fajr { get; }
    LocalTime? Shuruq { get; }
    LocalTime? Dhuhr { get; }
    LocalTime? Asr { get; }
    LocalTime? Maghrib { get; }
    LocalTime? Isha { get; }

    LocalTime? Jumuah { get; }
    LocalTime? Jumuah2 { get; }

    LocalTime? FajrCongregation { get; }
    LocalTime? DhuhrCongregation { get; }
    LocalTime? AsrCongregation { get; }
    LocalTime? MaghribCongregation { get; }
    LocalTime? IshaCongregation { get; }
}
