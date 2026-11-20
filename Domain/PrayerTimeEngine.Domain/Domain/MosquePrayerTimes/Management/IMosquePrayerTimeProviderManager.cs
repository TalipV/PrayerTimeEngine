using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;

public interface IMosquePrayerTimeProviderManager
{
    Task<MosquePrayerTimesDay> CalculatePrayerTimesAsync(int profileID, ZonedDateTime zoneDate, CancellationToken cancellationToken);
    Task<bool> ValidateData(EMosquePrayerTimeProviderType mosquePrayerTimeProviderType, string externalID, CancellationToken cancellationToken);

    /// <inheritdoc cref="Providers.IMosquePrayerTimeProvider.GetMosqueIdentityAsync"/>
    Task<MosqueIdentity?> GetMosqueIdentityAsync(EMosquePrayerTimeProviderType mosquePrayerTimeProviderType, string externalID, CancellationToken cancellationToken);
}
