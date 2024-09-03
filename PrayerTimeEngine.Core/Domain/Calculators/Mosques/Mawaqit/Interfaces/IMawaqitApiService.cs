using PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Interfaces
{
    public interface IMawaqitApiService
    {
        Task<MawaqitResponseDTO> GetPrayerTimesAsync(string externalID, CancellationToken cancellationToken);
    }
}
