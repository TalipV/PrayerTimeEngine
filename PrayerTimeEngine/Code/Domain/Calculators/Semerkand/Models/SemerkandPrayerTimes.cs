using Newtonsoft.Json;
using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Domain.CalculationService.Interfaces;

namespace PrayerTimeEngine.Code.Domain.Calculator.Semerkand.Models
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
