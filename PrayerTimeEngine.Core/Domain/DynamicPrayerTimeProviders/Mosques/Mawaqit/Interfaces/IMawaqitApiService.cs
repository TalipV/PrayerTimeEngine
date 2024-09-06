using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.Mawaqit.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.Mawaqit.Interfaces
{
    public interface IMawaqitApiService
    {
        Task<MawaqitResponseDTO> GetPrayerTimesAsync(string externalID, CancellationToken cancellationToken);
    }
}
