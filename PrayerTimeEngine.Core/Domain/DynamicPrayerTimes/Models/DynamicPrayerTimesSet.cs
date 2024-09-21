using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.Models.PrayerTimes;
using PropertyChanged;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

[AddINotifyPropertyChangedInterface]
public class DynamicPrayerTimesSet : IPrayerTimesSet
{
    public ZonedDateTime? DataCalculationTimestamp { get; set; }

    public List<(EPrayerType PrayerType, GenericPrayerTime Times)> AllPrayerTimes
    {
        get
        {
            return [
                (EPrayerType.Fajr, Fajr),
                (EPrayerType.Duha, Duha),
                (EPrayerType.Dhuhr, Dhuhr),
                (EPrayerType.Asr, Asr),
                (EPrayerType.Maghrib, Maghrib),
                (EPrayerType.Isha, Isha)
            ];
        }
    }

    public FajrPrayerTime Fajr { get; } = new();
    public DuhaPrayerTime Duha { get; } = new();
    public GenericPrayerTime Dhuhr { get; } = new();
    public AsrPrayerTime Asr { get; } = new();
    public MaghribPrayerTime Maghrib { get; } = new();
    public IshaPrayerTime Isha { get; } = new();

    public override bool Equals(object obj)
    {
        if (obj is not DynamicPrayerTimesSet otherDynamicPrayerTimesSet)
            return false;

        return Fajr.Equals(otherDynamicPrayerTimesSet.Fajr)
            && Duha.Equals(otherDynamicPrayerTimesSet.Duha)
            && Dhuhr.Equals(otherDynamicPrayerTimesSet.Dhuhr)
            && Asr.Equals(otherDynamicPrayerTimesSet.Asr)
            && Maghrib.Equals(otherDynamicPrayerTimesSet.Maghrib)
            && Isha.Equals(otherDynamicPrayerTimesSet.Isha);
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