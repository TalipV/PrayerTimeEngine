namespace PrayerTimeEngine.Core.Domain.Models;

/// <summary>
/// The profile has a place, but the location data of a provider could not be determined for it
/// (e.g. the provider's API or the place service was unavailable when the place was selected, or the
/// provider does not know the place). The times of that provider stay missing; the app tries to
/// determine the location data again in the background.
/// </summary>
public class LocationDataMissingException(string providerName)
    : Exception($"No location data of {providerName} for the selected place.")
{
    public string ProviderName { get; } = providerName;
}
