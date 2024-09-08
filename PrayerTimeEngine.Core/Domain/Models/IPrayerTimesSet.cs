using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models;

public interface IPrayerTimesSet
{
    ZonedDateTime? DataCalculationTimestamp { get; }
}
