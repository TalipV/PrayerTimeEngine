using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;

public interface IMosquePrayerTimeProviderManager
{
    public Task<MosquePrayerTimesDay> CalculatePrayerTimesAsync(int profileID, ZonedDateTime zoneDate, CancellationToken cancellationToken);
    Task<bool> ValidateData(EMosquePrayerTimeProviderType mosquePrayerTimeProviderType, string externalID, CancellationToken cancellationToken);
}
