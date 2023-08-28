using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.CalculationService.Interfaces;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models
{
    public class FaziletPrayerTimes : ICalculationPrayerTimes
    {
        public required DateTime Date { get; set; }
        public required int CityID { get; set; }

        public required DateTime Imsak { get; set; }
        public DateTime? NextFajr { get; set; }
        public required DateTime Fajr { get; set; }
        public required DateTime Shuruq { get; set; }
        public required DateTime Dhuhr { get; set; }
        public required DateTime Asr { get; set; }
        public required DateTime Maghrib { get; set; }
        public required DateTime Isha { get; set; }

        public DateTime GetDateTimeForTimeType(ETimeType timeType)
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
                    return NextFajr ?? DateTime.MinValue;
                default:
                    throw new ArgumentException($"Invalid {nameof(timeType)} value: {timeType}.");
            }
        }
    }
}
