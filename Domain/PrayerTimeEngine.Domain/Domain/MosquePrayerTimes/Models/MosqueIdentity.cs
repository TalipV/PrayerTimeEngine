namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

/// <summary>
/// A mosque as confirmed by its mosque app.
/// </summary>
/// <param name="ExternalID">The identifier in the form that is stored in the profile (e.g. Mawaqit's slug <c>assunnah-frechen</c>).</param>
/// <param name="Name">The mosque's name as the mosque app has it, or <c>null</c> when the app does not provide one.</param>
public sealed record MosqueIdentity(
    string ExternalID,
    string? Name);
