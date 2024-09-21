using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Domain.Models;

public interface IPrayerTimesSet
{
    ZonedDateTime? DataCalculationTimestamp { get; }

    List<(EPrayerType PrayerType, GenericPrayerTime Times)> AllPrayerTimes { get; }
}
