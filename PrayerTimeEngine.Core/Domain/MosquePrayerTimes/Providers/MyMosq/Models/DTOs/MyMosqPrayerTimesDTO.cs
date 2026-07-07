using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.JsonConverters;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.Entities;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.DTOs;

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

    // "XTime" holds the actual prayer time and the suffixless "X" holds the (possibly delayed)
    // congregation time (e.g. "Dhuhr" holds the jumu'ah time on Fridays, "Isha" is delayed in winter).
    // Only the fajr pair is named differently: "Fajr" = actual prayer time, "Sabah" = congregation.
    internal MyMosqMosqueDailyPrayerTimes ToMyMosqPrayerTimes(string externalID)
    {
        return new MyMosqMosqueDailyPrayerTimes
        {
            ExternalID = externalID.ToString(),
            Date = Date,

            Fajr = Fajr,
            Shuruq = Shuruq,
            Dhuhr = DhuhrTime,
            Asr = AsrTime,
            Maghrib = MaghribTime,
            Isha = IshaTime,

            Jumuah = Jumuah,
            Jumuah2 = Jumuah2,

            FajrCongregation = FajrTime,
            DhuhrCongregation = Dhuhr,
            AsrCongregation = Asr,
            MaghribCongregation = Maghrib,
            IshaCongregation = Isha,
        };
    }
}