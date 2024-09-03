using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.MyMosq.JsonConverters;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.MyMosq.Models.Entities;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.Calculators.Mosques.MyMosq.Models.DTOs;

public class MyMosqPrayerTimesDTO
{
    [JsonConverter(typeof(LocalDateConverter))]
    [JsonPropertyName("Date")]
    public LocalDate Date { get; set; }

    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("Fajr")]
    public LocalTime Fajr { get; set; }

    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("Sabah")]
    public LocalTime FajrTime { get; set; }

    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("Sunrise")]
    public LocalTime Shuruq { get; set; }


    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("Dhuhr")]
    public LocalTime Dhuhr { get; set; }

    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("DhuhrTime")]
    public LocalTime DhuhrTime { get; set; }


    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("Asr")]
    public LocalTime Asr { get; set; }

    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("AsrTime")]
    public LocalTime AsrTime { get; set; }


    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("Maghrib")]
    public LocalTime Maghrib { get; set; }

    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("MaghribTime")]
    public LocalTime MaghribTime { get; set; }


    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("Isha")]
    public LocalTime Isha { get; set; }

    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("IshaTime")]
    public LocalTime IshaTime { get; set; }

    [JsonConverter(typeof(NullableLocalTimeConverter))]
    [JsonPropertyName("Jummah2")]
    public LocalTime? Jumuah { get; set; }

    [JsonConverter(typeof(NullableLocalTimeConverter))]
    [JsonPropertyName("Jummah3")]
    public LocalTime? Jumuah2 { get; set; }

    internal MyMosqPrayerTimes ToMyMosqPrayerTimes(string externalID)
    {
        return new MyMosqPrayerTimes
        {
            ExternalID = externalID.ToString(),
            Date = this.Date,

            Fajr = this.Fajr,
            Shuruq = this.Shuruq,
            Dhuhr = this.Dhuhr,
            Asr = this.Asr,
            Maghrib = this.Maghrib,
            Isha = this.Isha,

            Jumuah = this.Jumuah,
            Jumuah2 = this.Jumuah2,

            FajrCongregation = this.FajrTime,
            DhuhrCongregation = this.DhuhrTime,
            AsrCongregation = this.AsrTime,
            MaghribCongregation = this.MaghribTime,
            IshaCongregation = this.IshaTime,
        };
    }
}