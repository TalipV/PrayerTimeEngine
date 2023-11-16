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
            switch (timeType)
            {
                case ETimeType.FajrStart:
                case ETimeType.FajrGhalas:
                case ETimeType.FajrKaraha:
                    return Fajr;
                case ETimeType.FajrEnd:
                    return Shuruq;
                case ETimeType.DuhaStart:
                    return Duha;
                case ETimeType.DhuhrStart:
                case ETimeType.DuhaEnd:
                    return Dhuhr;
                case ETimeType.DhuhrEnd:
                    return Asr;
                case ETimeType.AsrStart:
                    return Asr;
                case ETimeType.AsrEnd:
                    return Maghrib;
                case ETimeType.AsrMithlayn:
                    return AsrMithlayn;
                case ETimeType.AsrKaraha:
                    return AsrKaraha;
                case ETimeType.MaghribStart:
                    return Maghrib;
                case ETimeType.MaghribIshtibaq:
                    return Ishtibaq;
                case ETimeType.MaghribEnd:
                case ETimeType.IshaStart:
                    return Isha;
                case ETimeType.IshaEnd:
                    return NextFajr;
                default:
                    throw new ArgumentException($"Invalid {nameof(timeType)} value: {timeType}.");
            }
        }
    }
}
