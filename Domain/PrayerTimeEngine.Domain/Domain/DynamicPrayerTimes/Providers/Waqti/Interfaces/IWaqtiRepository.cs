using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Interfaces;

public interface IWaqtiRepository
{
    /// <summary>
    /// Returns the cached day of the given location and method, calculated with the given angle
    /// overrides (null: the angle of the method).
    /// </summary>
    Task<WaqtiDailyPrayerTimes?> GetPrayerTimesAsync(
        LocalDate date,
        decimal longitude,
        decimal latitude,
        string method,
        double? fajrAngle,
        double? ishaaAngle,
        CancellationToken cancellationToken);

    /// <summary>
    /// Inserts the given days, skipping the ones which are already stored for the same
    /// location, method and angle overrides.
    /// </summary>
    Task InsertPrayerTimesAsync(IEnumerable<WaqtiDailyPrayerTimes> waqtiPrayerTimesLst, CancellationToken cancellationToken);
}
