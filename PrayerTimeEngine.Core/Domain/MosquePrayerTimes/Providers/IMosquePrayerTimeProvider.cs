using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers;

public interface IMosquePrayerTimeProvider
{
    Task<IMosquePrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken);
}