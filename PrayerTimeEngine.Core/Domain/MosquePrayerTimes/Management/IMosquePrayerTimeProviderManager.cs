using NodaTime;
using PrayerTimeEngine.Core.Domain.Models;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;

public interface IMosquePrayerTimeProviderManager
{
    public Task<PrayerTimesCollection> CalculatePrayerTimesAsync(int profileID, ZonedDateTime zoneDate, CancellationToken cancellationToken);
}
