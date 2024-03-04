using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models
{
    public class MuwaqqitPrayerTimes : ICalculationPrayerTimes
    {
        [Key]
        public int ID { get; set; }

        public required LocalDate Date { get; set; }
        public required decimal Longitude { get; set; }
        public required decimal Latitude { get; set; }
        public Instant? InsertInstant { get; set; }

        public required double FajrDegree { get; set; }
        public required double AsrKarahaDegree { get; set; }
        public required double IshtibaqDegree { get; set; }
        public required double IshaDegree { get; set; }

        public required ZonedDateTime Fajr { get; set; }
        public required ZonedDateTime NextFajr { get; set; }
        public required ZonedDateTime Shuruq { get; set; }
        public required ZonedDateTime Duha { get; set; }
        public required ZonedDateTime Dhuhr { get; set; }
        public required ZonedDateTime Asr { get; set; }
        public required ZonedDateTime AsrMithlayn { get; set; }
        public required ZonedDateTime Maghrib { get; set; }
        public required ZonedDateTime Isha { get; set; }
        public required ZonedDateTime Ishtibaq { get; set; }
        public required ZonedDateTime AsrKaraha { get; set; }

        public ZonedDateTime GetZonedDateTimeForTimeType(ETimeType timeType)
        {
            return timeType switch
            {
                ETimeType.FajrStart or ETimeType.FajrGhalas or ETimeType.FajrKaraha => Fajr,
                ETimeType.FajrEnd => Shuruq,
                ETimeType.DuhaStart => Duha,
                ETimeType.DhuhrStart or ETimeType.DuhaEnd => Dhuhr,
                ETimeType.DhuhrEnd => Asr,
                ETimeType.AsrStart => Asr,
                ETimeType.AsrEnd => Maghrib,
                ETimeType.AsrMithlayn => AsrMithlayn,
                ETimeType.AsrKaraha => AsrKaraha,
                ETimeType.MaghribStart => Maghrib,
                ETimeType.MaghribIshtibaq => Ishtibaq,
                ETimeType.MaghribEnd or ETimeType.IshaStart => Isha,
                ETimeType.IshaEnd => NextFajr,
                _ => throw new ArgumentException($"Invalid {nameof(timeType)} value: {timeType}."),
            };
        }
    }
}
