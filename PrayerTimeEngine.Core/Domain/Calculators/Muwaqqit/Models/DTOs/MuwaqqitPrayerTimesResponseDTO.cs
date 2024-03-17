using NodaTime;
using PrayerTimeEngine.Core.Data.JsonSerialization;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models.Entities;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models.DTOs
{
    public class MuwaqqitPrayerTimesResponseDTO
    {
        [JsonPropertyName("fajr")]
        [JsonConverter(typeof(OffsetDateTimeConverter))]
        public OffsetDateTime Fajr { get; set; }

        [JsonPropertyName("fajr_t")]
        [JsonConverter(typeof(OffsetDateTimeConverter))]
        public OffsetDateTime NextFajr { get; set; }

        [JsonPropertyName("sunrise")]
        [JsonConverter(typeof(OffsetDateTimeConverter))]
        public OffsetDateTime Shuruq { get; set; }

        [JsonPropertyName("ishraq")]
        [JsonConverter(typeof(OffsetDateTimeConverter))]
        public OffsetDateTime Ishraq { get; set; }

        [JsonPropertyName("zohr")]
        [JsonConverter(typeof(OffsetDateTimeConverter))]
        public OffsetDateTime Dhuhr { get; set; }

        [JsonPropertyName("asr_shafi")]
        [JsonConverter(typeof(OffsetDateTimeConverter))]
        public OffsetDateTime Asr { get; set; }

        [JsonPropertyName("asr_hanafi")]
        [JsonConverter(typeof(OffsetDateTimeConverter))]
        public OffsetDateTime AsrMithlayn { get; set; }

        [JsonPropertyName("sunset")]
        [JsonConverter(typeof(OffsetDateTimeConverter))]
        public OffsetDateTime Maghrib { get; set; }

        [JsonPropertyName("ishtibak")]
        [JsonConverter(typeof(OffsetDateTimeConverter))]
        public OffsetDateTime Ishtibaq { get; set; }

        [JsonPropertyName("asr_makrooh")]
        [JsonConverter(typeof(OffsetDateTimeConverter))]
        public OffsetDateTime AsrKaraha { get; set; }

        [JsonPropertyName("esha")]
        [JsonConverter(typeof(OffsetDateTimeConverter))]
        public OffsetDateTime Isha { get; set; }


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

        public MuwaqqitPrayerTimes ToMuwaqqitPrayerTimes()
        {
            return new MuwaqqitPrayerTimes
            {
                Date = this.Date,
                Longitude = this.Longitude,
                Latitude = this.Latitude,

                FajrDegree = getRoundedDegreeValue(this.FajrDegree),
                AsrKarahaDegree = getRoundedDegreeValue(this.AsrKarahaDegree),
                IshtibaqDegree = getRoundedDegreeValue(this.IshtibaqDegree),
                IshaDegree = getRoundedDegreeValue(this.IshaDegree),

                Fajr = getZonedDateTime(this.Fajr, this.Timezone),
                NextFajr = getZonedDateTime(this.NextFajr, this.Timezone),
                Shuruq = getZonedDateTime(this.Shuruq, this.Timezone),
                Duha = getZonedDateTime(this.Ishraq, this.Timezone),
                Dhuhr = getZonedDateTime(this.Dhuhr, this.Timezone),
                Asr = getZonedDateTime(this.Asr, this.Timezone),
                AsrMithlayn = getZonedDateTime(this.AsrMithlayn, this.Timezone),
                Maghrib = getZonedDateTime(this.Maghrib, this.Timezone),
                Isha = getZonedDateTime(this.Isha, this.Timezone),
                Ishtibaq = getZonedDateTime(this.Ishtibaq, this.Timezone),
                AsrKaraha = getZonedDateTime(this.AsrKaraha, this.Timezone),
            };
        }

        private static double getRoundedDegreeValue(double degree)
        {
            return Math.Round(degree, 2);
        }

        private static ZonedDateTime getZonedDateTime(OffsetDateTime offsetDateTime, DateTimeZone timezone)
        {
            offsetDateTime.Deconstruct(out LocalDateTime localDateTime, out Offset offset);

            // ignore fractions of seconds
            localDateTime = localDateTime.Date + new LocalTime(localDateTime.Hour, localDateTime.Minute, localDateTime.Second);

            return new ZonedDateTime(
                localDateTime: localDateTime,
                zone: timezone,
                offset: offset);
        }
    }
}
