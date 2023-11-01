using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.CalculationService.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models
{
    public class FaziletPrayerTimes : ICalculationPrayerTimes
    {
        [Key]
        public int ID { get; set; }

        public required LocalDate Date { get; set; }
        public required int CityID { get; set; }

        public required ZonedDateTime Imsak { get; set; }
        public ZonedDateTime? NextFajr { get; set; }
        public required ZonedDateTime Fajr { get; set; }
        public required ZonedDateTime Shuruq { get; set; }
        public required ZonedDateTime Dhuhr { get; set; }
        public required ZonedDateTime Asr { get; set; }
        public required ZonedDateTime Maghrib { get; set; }
        public required ZonedDateTime Isha { get; set; }

        public ZonedDateTime GetZonedDateTimeForTimeType(ETimeType timeType)
        {
            switch (timeType)
            {
                case ETimeType.FajrStart:
                    return Fajr;
                case ETimeType.FajrEnd:
                    return Shuruq;

                case ETimeType.DuhaStart:
                    return Shuruq;

                case ETimeType.DhuhrStart:
                    return Dhuhr;
                case ETimeType.DhuhrEnd:
                    return Asr;

                case ETimeType.AsrStart:
                    return Asr;
                case ETimeType.AsrEnd:
                    return Maghrib;

                case ETimeType.MaghribStart:
                    return Maghrib;
                case ETimeType.MaghribEnd:
                    return Isha;
                case ETimeType.IshaStart:
                    return Isha;
                case ETimeType.IshaEnd:
                    return NextFajr ?? new ZonedDateTime(Instant.MinValue, DateTimeZone.Utc);
                default:
                    throw new ArgumentException($"Invalid {nameof(timeType)} value: {timeType}.");
            }
        }
    }
}
