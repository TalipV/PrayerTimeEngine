using NodaTime;
using PrayerTimeEngine.Core.Data.JsonSerialization;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.Entities;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.DTOs
{
    public class SemerkandPrayerTimesResponseDTO
    {
        [JsonPropertyName("DayOfYear")]
        public int DayOfYear { get; set; }

        [JsonPropertyName("Fajr")]
        [JsonConverter(typeof(LocalTimeConverter))]
        public LocalTime Fajr { get; set; }

        [JsonPropertyName("Tulu")]
        [JsonConverter(typeof(LocalTimeConverter))]
        public LocalTime Shuruq { get; set; }

        [JsonPropertyName("Zuhr")]
        [JsonConverter(typeof(LocalTimeConverter))]
        public LocalTime Dhuhr { get; set; }

        [JsonPropertyName("Asr")]
        [JsonConverter(typeof(LocalTimeConverter))]
        public LocalTime Asr { get; set; }

        [JsonPropertyName("Maghrib")]
        [JsonConverter(typeof(LocalTimeConverter))]
        public LocalTime Maghrib { get; set; }

        [JsonPropertyName("Isha")]
        [JsonConverter(typeof(LocalTimeConverter))]
        public LocalTime Isha { get; set; }

        internal SemerkandPrayerTimes ToSemerkandPrayerTimes(int cityID, DateTimeZone dateTimeZone, LocalDate firstDayOfYear)
        {
            LocalDate localDate = firstDayOfYear.PlusDays(this.DayOfYear - 1);

            return new SemerkandPrayerTimes
            {
                DayOfYear = this.DayOfYear,
                CityID = cityID,
                Date = localDate.AtStartOfDayInZone(dateTimeZone),

                Fajr = getZonedDateTime(dateTimeZone, localDate, this.Fajr),
                Shuruq = getZonedDateTime(dateTimeZone, localDate, this.Shuruq),
                Dhuhr = getZonedDateTime(dateTimeZone, localDate, this.Dhuhr),
                Asr = getZonedDateTime(dateTimeZone, localDate, this.Asr),
                Maghrib = getZonedDateTime(dateTimeZone, localDate, this.Maghrib),
                Isha = getZonedDateTime(dateTimeZone, localDate, this.Isha)
            };
        }

        private static ZonedDateTime getZonedDateTime(DateTimeZone timezone, LocalDate date, LocalTime time)
        {
            // InZoneStrictly throws an exception if the time is inacceptable,
            // like within the skipped hour of DST or ambiguous duplicate hour
            return (date + time).InZoneStrictly(timezone);
        }
    }
}
