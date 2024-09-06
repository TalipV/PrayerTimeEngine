using PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers.Mawaqit.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers.Mawaqit.Interfaces
{
    public interface IMawaqitApiService
    {
        Task<MawaqitResponseDTO> GetPrayerTimesAsync(string externalID, CancellationToken cancellationToken);
    }
}
