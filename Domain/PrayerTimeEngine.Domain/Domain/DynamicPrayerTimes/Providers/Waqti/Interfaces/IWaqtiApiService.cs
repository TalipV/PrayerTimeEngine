using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Interfaces;

// ONION port: technology-free contract. The concrete HTTP routing (Refit) lives in the
// Infrastructure adapter, so the Application ring depends only on this abstraction.
public interface IWaqtiApiService
{
    /// <summary>
    /// Returns the prayer times of every day of the requested month.
    /// </summary>
    Task<List<WaqtiDailyPrayerTimesResponseDTO>> GetMonthPrayerTimesAsync(
        WaqtiMonthPrayerTimesRequestDTO request,
        CancellationToken cancellationToken);
}
