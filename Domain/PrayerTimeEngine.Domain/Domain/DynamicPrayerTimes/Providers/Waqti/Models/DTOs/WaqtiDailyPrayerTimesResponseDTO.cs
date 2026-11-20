using System.Text.Json.Serialization;
using NodaTime;
using PrayerTimeEngine.Core.Data.JsonSerialization;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.DTOs;

/// <summary>
/// One day of the month response. The backend sends many more fields (solar positions, night
/// thirds, Hijri date, ...); only the ones the app uses are mapped.
/// </summary>
public class WaqtiDailyPrayerTimesResponseDTO
{
    [JsonPropertyName("date")]
    [JsonConverter(typeof(LocalDateConverter))]
    public LocalDate Date { get; set; }

    [JsonPropertyName("timezone")]
    [JsonConverter(typeof(DateTimeZoneConverter))]
    public DateTimeZone? Timezone { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("fajr")]
    public WaqtiTimeResponseDTO? Fajr { get; set; }

    [JsonPropertyName("shuruk")]
    public WaqtiTimeResponseDTO? Shuruk { get; set; }

    [JsonPropertyName("duha")]
    public WaqtiTimeResponseDTO? Duha { get; set; }

    [JsonPropertyName("dhuhr")]
    public WaqtiTimeResponseDTO? Dhuhr { get; set; }

    [JsonPropertyName("asr")]
    public WaqtiTimeResponseDTO? Asr { get; set; }

    [JsonPropertyName("asrTwoShadowLength")]
    public WaqtiTimeResponseDTO? AsrTwoShadowLength { get; set; }

    [JsonPropertyName("maghrib")]
    public WaqtiTimeResponseDTO? Maghrib { get; set; }

    [JsonPropertyName("ishaa")]
    public WaqtiTimeResponseDTO? Ishaa { get; set; }

    [JsonPropertyName("fajrTomorrow")]
    public WaqtiTimeResponseDTO? FajrTomorrow { get; set; }

    /// <summary>
    /// Maps the day to the cache entity. <paramref name="fajrAngle"/> and <paramref name="ishaaAngle"/>
    /// are the angle overrides of the request (null: the angles of the method).
    /// </summary>
    /// <remarks>
    /// A response of a request with angle overrides is only used for the times derived from these
    /// angles (Ghalas, Karaha, Ishtibak, ...). For them the exact moment matters, so the raw times
    /// without the safety minutes and the rounding of the method are taken. Times for which Waqti
    /// had to estimate a substitute because the sun does not reach the angle (validity other than
    /// <see cref="WaqtiTimeResponseDTO.VALID"/>) are dropped, like Muwaqqit omits them.
    /// </remarks>
    public WaqtiDailyPrayerTimes ToWaqtiPrayerTimes(decimal longitude, decimal latitude, double? fajrAngle = null, double? ishaaAngle = null)
    {
        bool isDerivedRequest = fajrAngle is not null || ishaaAngle is not null;

        Instant? GetTime(WaqtiTimeResponseDTO? time)
            => isDerivedRequest ? time?.GetRawInstantIfValid() : time?.GetInstant();

        return new WaqtiDailyPrayerTimes
        {
            Date = Date,
            TimeZone = Timezone ?? throw new InvalidOperationException($"Waqti response for {Date} contains no time zone."),
            Longitude = longitude,
            Latitude = latitude,
            Method = Method ?? string.Empty,
            FajrAngle = fajrAngle,
            IshaaAngle = ishaaAngle,

            Fajr = GetTime(Fajr),
            Shuruq = GetTime(Shuruk),
            Duha = GetTime(Duha),
            Dhuhr = GetTime(Dhuhr),
            Asr = GetTime(Asr),
            AsrMithlayn = GetTime(AsrTwoShadowLength),
            Maghrib = GetTime(Maghrib),
            Isha = GetTime(Ishaa),
            NextFajr = GetTime(FajrTomorrow),
        };
    }
}

/// <summary>
/// A single time of the response. <see cref="Instant"/> already contains the safety minutes
/// ("security") and the rounding of the calculation method, i.e. it is the time Waqti publishes.
/// <see cref="RawInstant"/> is the exact astronomical moment without them.
/// </summary>
public class WaqtiTimeResponseDTO
{
    public const string VALID = "Valid";

    [JsonPropertyName("instant")]
    public DateTimeOffset? Instant { get; set; }

    [JsonPropertyName("rawInstant")]
    public DateTimeOffset? RawInstant { get; set; }

    [JsonPropertyName("validity")]
    public string? Validity { get; set; }

    public Instant? GetInstant() => ToInstant(Instant);

    public Instant? GetRawInstantIfValid() => Validity == VALID ? ToInstant(RawInstant) : null;

    private static Instant? ToInstant(DateTimeOffset? dateTimeOffset)
    {
        if (dateTimeOffset is not DateTimeOffset value)
        {
            return null;
        }

        // ignore fractions of seconds
        return NodaTime.Instant.FromUnixTimeSeconds(value.ToUnixTimeSeconds());
    }
}
