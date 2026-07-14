using NodaTime;
using PrayerTimeEngine.Core.Data.JsonSerialization;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.DTOs;

public class SemerkandPrayerTimesResponseDTO
{
    [JsonPropertyName("DayOfYear")]
    public int DayOfYear { get; set; }

    [JsonPropertyName("Fajr")]
    [JsonConverter(typeof(LocalTimeConverter))]
    public LocalTime? Fajr { get; set; }

    [JsonPropertyName("Tulu")]
    [JsonConverter(typeof(LocalTimeConverter))]
    public LocalTime? Shuruq { get; set; }

    [JsonPropertyName("Zuhr")]
    [JsonConverter(typeof(LocalTimeConverter))]
    public LocalTime? Dhuhr { get; set; }

    [JsonPropertyName("Asr")]
    [JsonConverter(typeof(LocalTimeConverter))]
    public LocalTime? Asr { get; set; }

    [JsonPropertyName("Maghrib")]
    [JsonConverter(typeof(LocalTimeConverter))]
    public LocalTime? Maghrib { get; set; }

    [JsonPropertyName("Isha")]
    [JsonConverter(typeof(LocalTimeConverter))]
    public LocalTime? Isha { get; set; }

    internal SemerkandDailyPrayerTimes ToSemerkandPrayerTimes(int cityID, DateTimeZone dateTimeZone, LocalDate firstDayOfYear)
    {
        LocalDate localDate = firstDayOfYear.PlusDays(DayOfYear - 1);

        return new SemerkandDailyPrayerTimes
        {
            DayOfYear = DayOfYear,
            CityID = cityID,
            TimeZone = dateTimeZone,
            Date = localDate,

            Fajr = getLocalDateTime(localDate, Fajr),
            Shuruq = getLocalDateTime(localDate, Shuruq),
            Dhuhr = getLocalDateTime(localDate, Dhuhr),
            Asr = getLocalDateTime(localDate, Asr),
            Maghrib = getLocalDateTime(localDate, Maghrib),
            Isha = getLocalDateTime(localDate, Isha)
        };
    }

    private static LocalDateTime? getLocalDateTime(LocalDate date, LocalTime? time)
    {
        if (time is null)
            return null;

        return date + time.Value;
    }
}
