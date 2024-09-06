using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Models;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers
{
    public interface IMosquePrayerTimeProvider
    {
        Task<IMosquePrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken);
    }
}