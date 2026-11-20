using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.DTOs;
using Refit;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Services;

// Refit twin: the HTTP-specific contract. Infrastructure implementation detail.
// waqti.app has no official API; this is the endpoint its "Mosque Schedule Generator" page uses.
internal interface IWaqtiApiClient
{
    [Post("/api/quarkus/prayer/times_monthCustom")]
    Task<List<WaqtiDailyPrayerTimesResponseDTO>> GetMonthPrayerTimesAsync(
        [Body] WaqtiMonthPrayerTimesRequestDTO request,
        CancellationToken cancellationToken);
}

// Adapter: implements the Domain port by delegating to the Refit client.
internal sealed class WaqtiApiService(IWaqtiApiClient client) : IWaqtiApiService
{
    public Task<List<WaqtiDailyPrayerTimesResponseDTO>> GetMonthPrayerTimesAsync(
        WaqtiMonthPrayerTimesRequestDTO request,
        CancellationToken cancellationToken)
        => client.GetMonthPrayerTimesAsync(request, cancellationToken);
}
