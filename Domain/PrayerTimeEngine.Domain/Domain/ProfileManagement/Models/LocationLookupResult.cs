using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Models;

/// <summary>Result of looking up the location data of one provider for a place.</summary>
/// <param name="Provider">The provider the location data is for.</param>
/// <param name="LocationData">The found location data, null if none was found.</param>
/// <param name="Error">Why the look up failed; null if it worked (even if the provider does not know the place).</param>
public sealed record LocationLookupResult(
    EDynamicPrayerTimeProviderType Provider,
    BaseLocationData? LocationData,
    Exception? Error)
{
    public bool IsFound => LocationData is not null;

    /// <summary>The look up worked, but the provider does not know the place.</summary>
    public bool IsPlaceUnknown => LocationData is null && Error is null;
}
