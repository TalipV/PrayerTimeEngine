using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models;

/// <summary>
/// The profile has a place, but the location data of a provider is missing for it. The times of that
/// provider stay missing; the app tries to determine the location data again in the background.
/// </summary>
/// <param name="providerName">The provider without location data, e.g. "Semerkand".</param>
/// <param name="placeDescription">The selected place, e.g. "Deutschland, Köln".</param>
/// <param name="lastError">Why the last look up failed (null if it did not fail or was not done yet).</param>
/// <param name="isPlaceUnknown">The last look up worked, but the provider does not know the place.</param>
/// <param name="nextAttemptAt">When the location data is looked up again automatically (null: right now).</param>
public class LocationDataMissingException(
        string providerName,
        string placeDescription,
        Exception? lastError,
        bool isPlaceUnknown,
        Instant? nextAttemptAt)
    : Exception(CreateMessage(providerName, placeDescription, lastError, isPlaceUnknown), lastError)
{
    public string ProviderName { get; } = providerName;
    public string PlaceDescription { get; } = placeDescription;
    public Exception? LastError { get; } = lastError;
    public bool IsPlaceUnknown { get; } = isPlaceUnknown;
    public Instant? NextAttemptAt { get; } = nextAttemptAt;

    private static string CreateMessage(string providerName, string placeDescription, Exception? lastError, bool isPlaceUnknown)
    {
        string reason =
            isPlaceUnknown ? $"{providerName} does not know the place"
            : lastError is not null ? $"the look up failed: {lastError.Message}"
            : "the look up is still pending";

        return $"No location data of {providerName} for '{placeDescription}' ({reason}).";
    }
}
