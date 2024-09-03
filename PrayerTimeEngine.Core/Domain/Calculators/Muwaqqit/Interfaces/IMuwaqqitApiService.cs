using Refit;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces
{
    public interface IMuwaqqitApiService
    {
        [Get("/api2.json")]
        Task<MuwaqqitPrayerTimesResponseDTO> GetPrayerTimesAsync(
            [AliasAs("d")] string date,
            [AliasAs("ln")] decimal longitude,
            [AliasAs("lt")] decimal latitude,
            [AliasAs("tz")] string timezone,
            [AliasAs("fa")] double fajrDegree,
            [AliasAs("ia")] double asrKarahaDegree,
            [AliasAs("isn")] double ishtibaqDegree,
            [AliasAs("ea")] double ishaDegree,
            CancellationToken cancellationToken);
    }
}
