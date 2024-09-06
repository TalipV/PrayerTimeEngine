using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.MyMosq.Models.Entities;

public class MyMosqPrayerTimes : IMosquePrayerTimes, IInsertedAt
{
    [Key]
    public int ID { get; set; }
    public required string ExternalID { get; set; }
    public Instant? InsertInstant { get; set; }

    public required LocalDate Date { get; set; }

    public required LocalTime Fajr { get; set; }
    public required LocalTime Shuruq { get; set; }
    public required LocalTime Dhuhr { get; set; }
    public required LocalTime Asr { get; set; }
    public required LocalTime Maghrib { get; set; }
    public required LocalTime Isha { get; set; }

    public LocalTime? Jumuah { get; set; }
    public LocalTime? Jumuah2 { get; set; }

    public required LocalTime FajrCongregation { get; set; }
    public required LocalTime DhuhrCongregation { get; set; }
    public required LocalTime AsrCongregation { get; set; }
    public required LocalTime MaghribCongregation { get; set; }
    public required LocalTime IshaCongregation { get; set; }
}