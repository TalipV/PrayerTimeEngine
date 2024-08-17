using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.Entities
{
    public class SemerkandPrayerTimes : IPrayerTimes, IInsertedAt
    {
        [Key]
        public int ID { get; set; }

        public int DayOfYear { get; set; }
        public required ZonedDateTime Date { get; set; }
        public required int CityID { get; set; }
        public Instant? InsertInstant { get; set; }

        public required ZonedDateTime Fajr { get; set; }

        public required ZonedDateTime Shuruq { get; set; }

        public required ZonedDateTime Dhuhr { get; set; }

        public required ZonedDateTime Asr { get; set; }

        public required ZonedDateTime Maghrib { get; set; }

        public required ZonedDateTime Isha { get; set; }
        public ZonedDateTime? NextFajr { get; set; }

        public ZonedDateTime GetZonedDateTimeForTimeType(ETimeType timeType)
        {
            return timeType switch
            {
                ETimeType.FajrStart => Fajr,
                ETimeType.FajrEnd => Shuruq,
                ETimeType.DuhaStart => Shuruq,
                ETimeType.DhuhrStart => Dhuhr,
                ETimeType.DhuhrEnd => Asr,
                ETimeType.AsrStart => Asr,
                ETimeType.AsrEnd => Maghrib,
                ETimeType.MaghribStart => Maghrib,
                ETimeType.MaghribEnd => Isha,
                ETimeType.IshaStart => Isha,
                ETimeType.IshaEnd => NextFajr ?? new ZonedDateTime(Instant.MinValue, DateTimeZone.Utc),
                _ => throw new ArgumentException($"Invalid {nameof(timeType)} value: {timeType}."),
            };
        }
    }
}
