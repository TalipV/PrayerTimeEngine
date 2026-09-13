using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Interfaces;

// ONION port: technology-free contract. The concrete HTTP routing (Refit) lives in the
// Infrastructure adapter, so the Application ring depends only on this abstraction.
public interface IMuwaqqitApiService
{
    Task<MuwaqqitPrayerTimesResponseDTO> GetPrayerTimesAsync(
        string date,
        decimal longitude,
        decimal latitude,
        string timezone,
        double fajrDegree,
        double asrKarahaDegree,
        double ishtibakDegree,
        double ishaDegree,
        CancellationToken cancellationToken);
}
