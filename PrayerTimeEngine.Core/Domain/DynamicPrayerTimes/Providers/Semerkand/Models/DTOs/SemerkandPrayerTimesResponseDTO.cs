using NodaTime;
using PrayerTimeEngine.Core.Data.JsonSerialization;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.DTOs
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
            LocalDate localDate = firstDayOfYear.PlusDays(DayOfYear - 1);

            return new SemerkandPrayerTimes
            {
                DayOfYear = DayOfYear,
                CityID = cityID,
                Date = localDate.AtStartOfDayInZone(dateTimeZone),

                Fajr = getZonedDateTime(dateTimeZone, localDate, Fajr),
                Shuruq = getZonedDateTime(dateTimeZone, localDate, Shuruq),
                Dhuhr = getZonedDateTime(dateTimeZone, localDate, Dhuhr),
                Asr = getZonedDateTime(dateTimeZone, localDate, Asr),
                Maghrib = getZonedDateTime(dateTimeZone, localDate, Maghrib),
                Isha = getZonedDateTime(dateTimeZone, localDate, Isha)
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
