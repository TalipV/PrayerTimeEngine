using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Interfaces;

public interface IMawaqitApiService
{
    Task<MawaqitResponseDTO> GetPrayerTimesAsync(string externalID, CancellationToken cancellationToken);
    Task<bool> ValidateData(string externalID, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the slug (e.g. <c>assunnah-frechen</c>) of the mosque page behind <paramref name="externalID"/>,
    /// which may also be the page of Mawaqit's numeric mosque ID (<c>id/11504</c>), or <c>null</c> when there is no such mosque.
    /// </summary>
    Task<string?> GetSlugAsync(string externalID, CancellationToken cancellationToken);
}
