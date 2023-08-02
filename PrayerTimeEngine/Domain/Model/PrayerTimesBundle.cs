using PrayerTimeEngine.Common.Enum;
using PropertyChanged;

namespace PrayerTimeEngine.Domain.Model
{
    [AddINotifyPropertyChangedInterface]
    public class PrayerTimesBundle
    {
        public PrayerTimesBundle()
        {
            AllPrayerTimes = 
                new List<PrayerTime>
                {
                    Fajr, Duha, Dhuhr, Asr, Maghrib, Isha
                }.AsReadOnly();
        }
        public IReadOnlyList<PrayerTime> AllPrayerTimes { get; init; }

        public FajrPrayerTime Fajr { get; private set; } = new ();
        public DuhaPrayerTime Duha { get; private set; } = new ();
        public DhuhrPrayerTime Dhuhr { get; private set; } = new();
        public AsrPrayerTime Asr { get; private set; } = new();
        public MaghribPrayerTime Maghrib { get; private set; } = new();
        public IshaPrayerTime Isha { get; private set; } = new();

        public void SetSpecificPrayerTimeDateTime(ETimeType timeType, DateTime? dateTime)
        {
            switch (timeType)
            {
                case ETimeType.FajrStart:
                    Fajr.Start = dateTime;
                    break;
                case ETimeType.FajrEnd:
                    Fajr.End = dateTime;
                    break;
                case ETimeType.FajrGhalas:
                    Fajr.Ghalas = dateTime;
                    break;
                case ETimeType.FajrKaraha:
                    Fajr.Karaha = dateTime;
                    break;

                case ETimeType.DuhaStart:
                    Duha.Start = dateTime;
                    break;
                case ETimeType.DuhaEnd:
                    Duha.End = dateTime;
                    break;
                case ETimeType.DuhaQuarterOfDay:
                    Duha.QuarterOfDay = dateTime;
                    break;

                case ETimeType.DhuhrStart:
                    Dhuhr.Start = dateTime;
                    break;
                case ETimeType.DhuhrEnd:
                    Dhuhr.End = dateTime;
                    break;

                case ETimeType.AsrStart:
                    Asr.Start = dateTime;
                    break;
                case ETimeType.AsrEnd:
                    Asr.End = dateTime;
                    break;
                case ETimeType.AsrMithlayn:
                    Asr.Mithlayn = dateTime;
                    break;
                case ETimeType.AsrKaraha:
                    Asr.Karaha = dateTime;
                    break;

                case ETimeType.MaghribStart:
                    Maghrib.Start = dateTime;
                    break;
                case ETimeType.MaghribEnd:
                    Maghrib.End = dateTime;
                    break;
                case ETimeType.MaghribSufficientTime:
                    Maghrib.SufficientTime = dateTime;
                    break;
                case ETimeType.MaghribIshtibaq:
                    Maghrib.Ishtibaq = dateTime;
                    break;

                case ETimeType.IshaStart:
                    Isha.Start = dateTime;
                    break;
                case ETimeType.IshaEnd:
                    Isha.End = dateTime;
                    break;
                case ETimeType.IshaFirstThird:
                    Isha.FirstThirdOfNight = dateTime;
                    break;
                case ETimeType.IshaMidnight:
                    Isha.MiddleOfNight = dateTime;
                    break;
                case ETimeType.IshaSecondThird:
                    Isha.SecondThirdOfNight = dateTime;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}