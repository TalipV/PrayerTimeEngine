using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers;

public interface IMosquePrayerTimeProvider
{
    Task<IMosqueDailyPrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken);
    Task<bool> ValidateData(string externalID, CancellationToken cancellationToken);

    /// <summary>
    /// Turns an identifier taken from user input into the form that is stored in the profile,
    /// or returns <c>null</c> when the mosque does not exist.
    /// </summary>
    Task<string?> NormalizeExternalIDAsync(string externalID, CancellationToken cancellationToken);
}