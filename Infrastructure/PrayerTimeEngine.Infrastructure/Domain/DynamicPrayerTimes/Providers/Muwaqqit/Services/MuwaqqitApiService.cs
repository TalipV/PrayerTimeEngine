using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.DTOs;
using Refit;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Services;

// Refit twin: the HTTP-specific contract. Infrastructure implementation detail.
internal interface IMuwaqqitApiClient
{
    [Get("/api2.json")]
    Task<MuwaqqitPrayerTimesResponseDTO> GetPrayerTimesAsync(
        [AliasAs("d")] string date,
        [AliasAs("ln")] decimal longitude,
        [AliasAs("lt")] decimal latitude,
        [AliasAs("tz")] string timezone,
        [AliasAs("fa")] double fajrDegree,
        [AliasAs("ia")] double asrKarahaDegree,
        [AliasAs("isn")] double ishtibakDegree,
        [AliasAs("ea")] double ishaDegree,
        CancellationToken cancellationToken);
}

// Adapter: implements the Domain port by delegating to the Refit client.
internal sealed class MuwaqqitApiService(IMuwaqqitApiClient client) : IMuwaqqitApiService
{
    public Task<MuwaqqitPrayerTimesResponseDTO> GetPrayerTimesAsync(
        string date,
        decimal longitude,
        decimal latitude,
        string timezone,
        double fajrDegree,
        double asrKarahaDegree,
        double ishtibakDegree,
        double ishaDegree,
        CancellationToken cancellationToken)
        => client.GetPrayerTimesAsync(date, longitude, latitude, timezone, fajrDegree, asrKarahaDegree, ishtibakDegree, ishaDegree, cancellationToken);
}
