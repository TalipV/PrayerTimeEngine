using NodaTime;
using PrayerTimeEngine.Core.Data.JsonSerialization;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.Entities;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.DTOs;

public class MuwaqqitPrayerTimesResponseDTO
{
    [JsonPropertyName("fajr")]
    [JsonConverter(typeof(OffsetDateTimeConverter))]
    public OffsetDateTime? Fajr { get; set; }

    [JsonPropertyName("fajr_t")]
    [JsonConverter(typeof(OffsetDateTimeConverter))]
    public OffsetDateTime? NextFajr { get; set; }

    [JsonPropertyName("sunrise")]
    [JsonConverter(typeof(OffsetDateTimeConverter))]
    public OffsetDateTime? Shuruq { get; set; }

    [JsonPropertyName("ishraq")]
    [JsonConverter(typeof(OffsetDateTimeConverter))]
    public OffsetDateTime? Ishraq { get; set; }

    [JsonPropertyName("zohr")]
    [JsonConverter(typeof(OffsetDateTimeConverter))]
    public OffsetDateTime? Dhuhr { get; set; }

    [JsonPropertyName("asr_shafi")]
    [JsonConverter(typeof(OffsetDateTimeConverter))]
    public OffsetDateTime? Asr { get; set; }

    [JsonPropertyName("asr_hanafi")]
    [JsonConverter(typeof(OffsetDateTimeConverter))]
    public OffsetDateTime? AsrMithlayn { get; set; }

    [JsonPropertyName("sunset")]
    [JsonConverter(typeof(OffsetDateTimeConverter))]
    public OffsetDateTime? Maghrib { get; set; }

    [JsonPropertyName("ishtibak")]
    [JsonConverter(typeof(OffsetDateTimeConverter))]
    public OffsetDateTime? Ishtibaq { get; set; }

    [JsonPropertyName("asr_makrooh")]
    [JsonConverter(typeof(OffsetDateTimeConverter))]
    public OffsetDateTime? AsrKaraha { get; set; }

    [JsonPropertyName("esha")]
    [JsonConverter(typeof(OffsetDateTimeConverter))]
    public OffsetDateTime? Isha { get; set; }


    [JsonPropertyName("ln")]
    public decimal Longitude { get; set; }

    [JsonPropertyName("lt")]
    public decimal Latitude { get; set; }


    [JsonPropertyName("ea")]
    public double IshaDegree { get; set; }

    [JsonPropertyName("fa")]
    public double FajrDegree { get; set; }

    [JsonPropertyName("asr_makrooh_alt")]
    public double AsrKarahaDegree { get; set; }

    [JsonPropertyName("ishtibak_alt")]
    public double IshtibaqDegree { get; set; }


    [JsonPropertyName("tz")]
    [JsonConverter(typeof(DateTimeZoneConverter))]
    public DateTimeZone Timezone { get; set; }

    [JsonPropertyName("d")]
    [JsonConverter(typeof(LocalDateConverter))]
    public LocalDate Date { get; set; }

    public MuwaqqitDailyPrayerTimes ToMuwaqqitPrayerTimes()
    {
        return new MuwaqqitDailyPrayerTimes
        {
            Date = Date,
            TimeZone = Timezone,
            Longitude = Longitude,
            Latitude = Latitude,

            FajrDegree = getRoundedDegreeValue(FajrDegree),
            AsrKarahaDegree = getRoundedDegreeValue(AsrKarahaDegree),
            IshtibaqDegree = getRoundedDegreeValue(IshtibaqDegree),
            IshaDegree = getRoundedDegreeValue(IshaDegree),

            Fajr = getInstant(Fajr),
            NextFajr = getInstant(NextFajr),
            Shuruq = getInstant(Shuruq),
            Duha = getInstant(Ishraq),
            Dhuhr = getInstant(Dhuhr),
            Asr = getInstant(Asr),
            AsrMithlayn = getInstant(AsrMithlayn),
            Maghrib = getInstant(Maghrib),
            Isha = getInstant(Isha),
            Ishtibaq = getInstant(Ishtibaq),
            AsrKaraha = getInstant(AsrKaraha),
        };
    }

    private static double getRoundedDegreeValue(double degree)
    {
        return Math.Round(degree, 2);
    }

    private static Instant? getInstant(OffsetDateTime? offsetDateTimeNullable)
    {
        if (offsetDateTimeNullable is not OffsetDateTime offsetDateTime)
            return null;

        // ignore fractions of seconds
        return Instant.FromUnixTimeSeconds(offsetDateTime.ToInstant().ToUnixTimeSeconds());
    }
}
