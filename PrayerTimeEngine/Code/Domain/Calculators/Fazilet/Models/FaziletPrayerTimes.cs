using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Domain.CalculationService.Interfaces;

namespace PrayerTimeEngine.Code.Domain.Calculator.Fazilet.Models
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
                    return this.Fajr;
                case ETimeType.FajrEnd:
                    return this.Shuruq;

                case ETimeType.DuhaStart:
                    return this.Shuruq;

                case ETimeType.DhuhrStart:
                    return this.Dhuhr;
                case ETimeType.DhuhrEnd:
                    return this.Asr;

                case ETimeType.AsrStart:
                    return this.Asr;
                case ETimeType.AsrEnd:
                    return this.Maghrib;

                case ETimeType.MaghribStart:
                    return this.Maghrib;
                case ETimeType.MaghribEnd:
                    return this.Isha;
                case ETimeType.IshaStart:
                    return this.Isha;
                case ETimeType.IshaEnd:
                    return this.NextFajr ?? DateTime.MinValue;
                default:
                    throw new ArgumentException($"Invalid {nameof(timeType)} value: {timeType}.");
            }
        }
    }
}
