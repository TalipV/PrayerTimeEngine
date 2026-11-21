using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models;

/// <summary>
/// The provider has no prayer times for the requested day, e.g. because its data only covers a
/// limited range of years. Not an error of the provider or the app: the times of that day simply
/// stay missing and the user gets a hint instead of an error message.
/// </summary>
public class PrayerTimesNotAvailableForDateException(LocalDate date, string providerName)
    : Exception($"{providerName} does not provide prayer times for {date:uuuu-MM-dd}.")
{
    public LocalDate Date { get; } = date;
    public string ProviderName { get; } = providerName;
}
