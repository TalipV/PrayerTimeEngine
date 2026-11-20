namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

/// <summary>
/// A mosque recognized in user input (a pasted browser link, a text shared from a mosque app or a plain identifier).
/// It only identifies the mosque; its name comes from the mosque app itself (<see cref="MosqueIdentity"/>),
/// so that recognition does not depend on the language of a shared text.
/// </summary>
/// <param name="ProviderType">
/// The mosque app the input belongs to, or <see cref="EMosquePrayerTimeProviderType.None"/> when the input is a plain
/// identifier that does not reveal its app.
/// </param>
/// <param name="ExternalID">
/// The identifier as found in the input. It may still need to be normalized by the provider before it is stored
/// (e.g. Mawaqit's <c>id/11504</c>, which is resolved to the mosque's slug).
/// </param>
public sealed record MosqueReference(
    EMosquePrayerTimeProviderType ProviderType,
    string ExternalID);
