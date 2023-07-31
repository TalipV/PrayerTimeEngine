using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models
{
    public class MuwaqqitPrayerTimes : ICalculationPrayerTimes
    {
        public required DateTime Date { get; set; }
        public required decimal Longitude { get; set; }
        public required decimal Latitude { get; set; }

        public required DateTime Fajr { get; set; }
        public required DateTime NextFajr { get; set; }
        public required DateTime Shuruq { get; set; }
        public required DateTime Duha { get; set; }
        public required DateTime Dhuhr { get; set; }
        public required DateTime Asr { get; set; }
        public required DateTime AsrMithlayn { get; set; }
        public required DateTime Maghrib { get; set; }
        public required DateTime Isha { get; set; }
        public required DateTime Ishtibaq { get; set; }
        public required DateTime AsrKaraha { get; set; }

        public DateTime GetDateTimeForTimeType(ETimeType timeType)
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
