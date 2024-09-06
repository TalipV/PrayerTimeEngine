using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers.MyMosq.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers.MyMosq.Interfaces
{
    public interface IMyMosqApiService
    {
        Task<List<MyMosqPrayerTimesDTO>> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken);
    }
}
