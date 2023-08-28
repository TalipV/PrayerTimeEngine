using Newtonsoft.Json;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.CalculationService.Interfaces;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models
{
    public class SemerkandPrayerTimes : ICalculationPrayerTimes
    {
        public required int CityID { get; set; }

        [JsonProperty("DayOfYear")]
        public int DayOfYear { get; set; }
        public required DateTime Date { get; set; }

        [JsonProperty("Fajr")]
        public required DateTime Fajr { get; set; }
        public DateTime? NextFajr { get; set; }

        [JsonProperty("Tulu")]
        public required DateTime Shuruq { get; set; }

        [JsonProperty("Zuhr")]
        public required DateTime Dhuhr { get; set; }

        [JsonProperty("Asr")]
        public required DateTime Asr { get; set; }

        [JsonProperty("Maghrib")]
        public required DateTime Maghrib { get; set; }

        [JsonProperty("Isha")]
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
