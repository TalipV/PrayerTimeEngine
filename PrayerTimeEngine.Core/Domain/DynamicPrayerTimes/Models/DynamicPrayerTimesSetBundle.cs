using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PropertyChanged;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

[AddINotifyPropertyChangedInterface]
public class DynamicPrayerTimesDaySet : IPrayerTimesDay
{
    public ZonedDateTime? DataCalculationTimestamp { get; set; }

    public DynamicPrayerTimesDay PreviousDay { get; set; }
    public DynamicPrayerTimesDay CurrentDay { get; set; }
    public DynamicPrayerTimesDay NextDay { get; set; }

    public List<(EPrayerType, GenericPrayerTime)> AllPrayerTimes
    {
        get
        {
            // Sorting?
            // Not creating a new list each time?
            return this.PreviousDay.AllPrayerTimes
                .Concat(this.CurrentDay.AllPrayerTimes)
                .Concat(this.NextDay.AllPrayerTimes)
                .Select(x => (PrayerType: x.Item1, Times: x.Item2))
                .ToList();
        }
    }

    #region System.Object overrides

    public override bool Equals(object obj)
    {
        if (obj is not DynamicPrayerTimesDaySet otherDynamicPrayerTimesSet)
            return false;

        return this.Equals(otherDynamicPrayerTimesSet.CurrentDay)
            && Equals(PreviousDay, otherDynamicPrayerTimesSet.PreviousDay)
            && Equals(NextDay, otherDynamicPrayerTimesSet.NextDay);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PreviousDay, CurrentDay, NextDay);
    }

    #endregion System.Object overrides
}

