using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PropertyChanged;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.Model
{
    [AddINotifyPropertyChangedInterface]
    public class PrayerTimesBundle
    {

        [JsonIgnore]
        public List<PrayerTime> AllPrayerTimes
        {
            get
            {
                return new List<PrayerTime>
                {
                    Fajr, Duha, Dhuhr, Asr, Maghrib, Isha
                };
            }
        }

        public FajrPrayerTime Fajr { get; init; } = new();
        public DuhaPrayerTime Duha { get; init; } = new();
        public DhuhrPrayerTime Dhuhr { get; init; } = new();
        public AsrPrayerTime Asr { get; init; } = new();
        public MaghribPrayerTime Maghrib { get; init; } = new();
        public IshaPrayerTime Isha { get; init; } = new();

        public override bool Equals(object obj)
        {
            if (obj is not PrayerTimesBundle otherPrayerTimeBundle)
                return false;

            return
                this.Fajr.Equals(otherPrayerTimeBundle.Fajr)
                && this.Duha.Equals(otherPrayerTimeBundle.Duha)
                && this.Dhuhr.Equals(otherPrayerTimeBundle.Dhuhr)
                && this.Asr.Equals(otherPrayerTimeBundle.Asr)
                && this.Maghrib.Equals(otherPrayerTimeBundle.Maghrib)
                && this.Isha.Equals(otherPrayerTimeBundle.Isha);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Fajr, Duha, Dhuhr, Asr, Maghrib, Isha);
        }

        public void SetSpecificPrayerTimeDateTime(ETimeType timeType, ZonedDateTime? zonedDateTime)
        {
            switch (timeType)
            {
                case ETimeType.FajrStart:
                    Fajr.Start = zonedDateTime;
                    break;
                case ETimeType.FajrEnd:
                    Fajr.End = zonedDateTime;
                    break;
                case ETimeType.FajrGhalas:
                    Fajr.Ghalas = zonedDateTime;
                    break;
                case ETimeType.FajrKaraha:
                    Fajr.Karaha = zonedDateTime;
                    break;

                case ETimeType.DuhaStart:
                    Duha.Start = zonedDateTime;
                    break;
                case ETimeType.DuhaEnd:
                    Duha.End = zonedDateTime;
                    break;
                case ETimeType.DuhaQuarterOfDay:
                    Duha.QuarterOfDay = zonedDateTime;
                    break;

                case ETimeType.DhuhrStart:
                    Dhuhr.Start = zonedDateTime;
                    break;
                case ETimeType.DhuhrEnd:
                    Dhuhr.End = zonedDateTime;
                    break;

                case ETimeType.AsrStart:
                    Asr.Start = zonedDateTime;
                    break;
                case ETimeType.AsrEnd:
                    Asr.End = zonedDateTime;
                    break;
                case ETimeType.AsrMithlayn:
                    Asr.Mithlayn = zonedDateTime;
                    break;
                case ETimeType.AsrKaraha:
                    Asr.Karaha = zonedDateTime;
                    break;

                case ETimeType.MaghribStart:
                    Maghrib.Start = zonedDateTime;
                    break;
                case ETimeType.MaghribEnd:
                    Maghrib.End = zonedDateTime;
                    break;
                case ETimeType.MaghribSufficientTime:
                    Maghrib.SufficientTime = zonedDateTime;
                    break;
                case ETimeType.MaghribIshtibaq:
                    Maghrib.Ishtibaq = zonedDateTime;
                    break;

                case ETimeType.IshaStart:
                    Isha.Start = zonedDateTime;
                    break;
                case ETimeType.IshaEnd:
                    Isha.End = zonedDateTime;
                    break;
                case ETimeType.IshaFirstThird:
                    Isha.FirstThirdOfNight = zonedDateTime;
                    break;
                case ETimeType.IshaMidnight:
                    Isha.MiddleOfNight = zonedDateTime;
                    break;
                case ETimeType.IshaSecondThird:
                    Isha.SecondThirdOfNight = zonedDateTime;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}