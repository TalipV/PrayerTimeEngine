using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PropertyChanged;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

[AddINotifyPropertyChangedInterface]
public class MosquePrayerTimesDay : IPrayerTimesDay
{
    public ZonedDateTime? DataCalculationTimestamp { get; set; }

    public List<(EPrayerType PrayerType, GenericPrayerTime Times)> AllPrayerTimes
    {
        get
        {
            return [
                (EPrayerType.Fajr, Fajr),
                (EPrayerType.Dhuhr, Dhuhr),
                (EPrayerType.Asr, Asr),
                (EPrayerType.Maghrib, Maghrib),
                (EPrayerType.Isha, Isha)
            ];
        }
    }

    public MosquePrayerTime Fajr { get; init; } = new();
    public MosquePrayerTime Dhuhr { get; init; } = new();
    public MosquePrayerTime Asr { get; init; } = new();
    public MosquePrayerTime Maghrib { get; init; } = new();
    public MosquePrayerTime Isha { get; init; } = new();
    public MosquePrayerTime Jumuah { get; init; } = new();
    public MosquePrayerTime Jumuah2 { get; init; } = new();

    public override bool Equals(object obj)
    {
        if (obj is not MosquePrayerTimesDay otherMosquePrayerTimesSet)
            return false;

        return Equals(Fajr, otherMosquePrayerTimesSet.Fajr)
            && Equals(Dhuhr, otherMosquePrayerTimesSet.Dhuhr)
            && Equals(Asr, otherMosquePrayerTimesSet.Asr)
            && Equals(Maghrib, otherMosquePrayerTimesSet.Maghrib)
            && Equals(Isha, otherMosquePrayerTimesSet.Isha)
            && Equals(Jumuah, otherMosquePrayerTimesSet.Jumuah)
            && Equals(Jumuah2, otherMosquePrayerTimesSet.Jumuah2);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Fajr, Dhuhr, Asr, Maghrib, Isha, HashCode.Combine(Jumuah, Jumuah2));
    }
}