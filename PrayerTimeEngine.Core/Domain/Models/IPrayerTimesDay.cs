using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Domain.Models;

public interface IPrayerTimesDay
{
    ZonedDateTime? DataCalculationTimestamp { get; }

    List<(ETimeSection Section, GenericPrayerTime Times)> AllPrayerTimes { get; }
}
