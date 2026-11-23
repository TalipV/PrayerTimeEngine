using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;

/// <summary>Outcome of the last location data look up of one provider for one profile.</summary>
/// <param name="AttemptedAt">When the look up was done.</param>
/// <param name="ConsecutiveFailureCount">Failed look ups in a row (errors only, not "place unknown").</param>
/// <param name="IsPlaceUnknown">The look up worked, but the provider does not know the place.</param>
/// <param name="LastError">Why the look up failed, null if it did not fail.</param>
public sealed record LocationLookupStatus(
    Instant AttemptedAt,
    int ConsecutiveFailureCount,
    bool IsPlaceUnknown,
    Exception? LastError);

/// <summary>
/// Remembers the outcome of the location data look ups per profile and provider (in memory), so the
/// app can explain why location data is missing and when it is looked up again.
/// Must be registered as a singleton so that all (transient) consumers share the same state.
/// </summary>
public interface ILocationLookupStatusStore
{
    LocationLookupStatus? Get(int profileID, EDynamicPrayerTimeProviderType provider);

    /// <summary>Records a look up: found (no error, not unknown), place unknown or failed with an error.</summary>
    void Record(int profileID, EDynamicPrayerTimeProviderType provider, bool isPlaceUnknown, Exception? error, Instant attemptedAt);

    /// <summary>Forgets everything about the profile, e.g. because another place was selected.</summary>
    void Clear(int profileID);
}
