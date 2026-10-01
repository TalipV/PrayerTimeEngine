using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

public class DynamicPrayerTimesDaySet : IPrayerTimesDay
{
    public ZonedDateTime? DataCalculationTimestamp { get; set; }

    public DynamicPrayerTimesDay PreviousDay { get; set; }
    public DynamicPrayerTimesDay CurrentDay { get; set; }
    public DynamicPrayerTimesDay NextDay { get; set; }

    public List<(ETimeSection Section, GenericPrayerTime Times)> AllPrayerTimes
    {
        get
        {
            // Sorting?
            // Not creating a new list each time?
            return [.. PreviousDay.AllPrayerTimes
                .Concat(CurrentDay.AllPrayerTimes)
                .Concat(NextDay.AllPrayerTimes)
                .Select(x => (x.Section, x.Times))];
        }
    }

    #region System.Object overrides

    public override bool Equals(object obj)
    {
        if (obj is not DynamicPrayerTimesDaySet otherDynamicPrayerTimesSet)
        {
            return false;
        }

        return Equals(otherDynamicPrayerTimesSet.CurrentDay)
            && Equals(PreviousDay, otherDynamicPrayerTimesSet.PreviousDay)
            && Equals(NextDay, otherDynamicPrayerTimesSet.NextDay);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PreviousDay, CurrentDay, NextDay);
    }

    #endregion System.Object overrides
}

