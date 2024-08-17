using NodaTime;
using PrayerTimeEngine.Core.Data.JsonSerialization;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.Entities;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.DTOs
{
    public class FaziletGetTimesByCityIDResponseDTO
    {
        [JsonPropertyName("bolge_saatdilimi")]
        [JsonConverter(typeof(DateTimeZoneConverter))]
        public DateTimeZone Timezone { get; set; }

        [JsonPropertyName("vakitler")]
        public List<FaziletPrayerTimesResponseDTO> PrayerTimes { get; set;}
    }

    public class FaziletPrayerTimesResponseDTO
    {
        [JsonPropertyName("tarih")]
        [JsonConverter(typeof(LocalDateConverter))]
        public LocalDate Date { get; set; }

        [JsonPropertyName("imsak")]
        public List<TimeDetail> Imsak { get; set; }

        [JsonPropertyName("sabah")]
        public List<TimeDetail> Fajr { get; set; }

        [JsonPropertyName("gunes")]
        public List<TimeDetail> Shuruq { get; set; }

        [JsonPropertyName("ogle")]
        public List<TimeDetail> Dhuhr { get; set; }

        [JsonPropertyName("ikindi")]
        public List<TimeDetail> Asr { get; set; }

        [JsonPropertyName("aksam")]
        public List<TimeDetail> Maghrib { get; set; }

        [JsonPropertyName("yatsi")]
        public List<TimeDetail> Isha { get; set; }

        internal FaziletPrayerTimes ToFaziletPrayerTimes(int cityID, DateTimeZone timeZone)
        {
            return new FaziletPrayerTimes
            {
                CityID = cityID,
                Imsak = this.Imsak.First().OffsetDateTime.InZone(timeZone),
                Fajr = this.Fajr.First().OffsetDateTime.InZone(timeZone),
                Shuruq = this.Shuruq.First().OffsetDateTime.InZone(timeZone),
                Dhuhr = this.Dhuhr.First().OffsetDateTime.InZone(timeZone),
                Asr = this.Asr.First().OffsetDateTime.InZone(timeZone),
                Maghrib = this.Maghrib.First().OffsetDateTime.InZone(timeZone),
                Isha = this.Isha.First().OffsetDateTime.InZone(timeZone),
                Date = this.Date.AtStartOfDayInZone(timeZone),
            };
        }
    }
    public class TimeDetail
    {
        [JsonPropertyName("tarih")]
        [JsonConverter(typeof(OffsetDateTimeConverter))]
        public OffsetDateTime OffsetDateTime { get; set; }
    }
}
