namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

/// <summary>
/// A mosque recognized in user input (a pasted browser link, a text shared from a mosque app or a plain identifier).
/// </summary>
/// <param name="ProviderType">
/// The mosque app the input belongs to, or <see cref="EMosquePrayerTimeProviderType.None"/> when the input is a plain
/// identifier that does not reveal its app.
/// </param>
/// <param name="ExternalID">
/// The identifier as found in the input. It may still need to be normalized by the provider before it is stored
/// (e.g. Mawaqit's <c>id/11504</c>, which is resolved to the mosque's slug).
/// </param>
/// <param name="MosqueName">The mosque's name, when the input contains it (e.g. a text shared from the Mawaqit app).</param>
public sealed record MosqueReference(
    EMosquePrayerTimeProviderType ProviderType,
    string ExternalID,
    string? MosqueName = null);
