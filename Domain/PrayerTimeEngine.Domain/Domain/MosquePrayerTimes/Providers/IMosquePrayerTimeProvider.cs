using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers;

public interface IMosquePrayerTimeProvider
{
    Task<IMosqueDailyPrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken);
    Task<bool> ValidateData(string externalID, CancellationToken cancellationToken);

    /// <summary>
    /// Looks up the mosque behind an identifier taken from user input: the identifier in the form that is stored
    /// in the profile and, where the mosque app provides it, the mosque's name. <c>null</c> when the mosque does not exist.
    /// </summary>
    Task<MosqueIdentity?> GetMosqueIdentityAsync(string externalID, CancellationToken cancellationToken);
}