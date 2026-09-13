namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;

/// <summary>
/// Tracks an increasing counter per profile, bumped on every mutation.
/// Allows cache validity to be checked without a DB round-trip.
/// Must be registered as a singleton so that all (transient) consumers share the same state.
/// </summary>
public interface IProfileVersionStore
{
    long GetVersion(int profileID);
    void BumpVersion(int profileID);
}
