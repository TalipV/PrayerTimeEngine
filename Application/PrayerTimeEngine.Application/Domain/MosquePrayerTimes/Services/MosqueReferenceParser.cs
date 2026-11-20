using System.Text.RegularExpressions;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Services;

/// <summary>
/// Finds the mosque in what the user pasted or shared into the app:
/// <list type="bullet">
/// <item>the text of the Mawaqit app's "Share" button (mosque name, website, <c>https://mawaqit.net/en/id/11504</c>, ...)</item>
/// <item>a Mawaqit browser link (<c>https://mawaqit.net/de/assunnah-frechen</c>)</item>
/// <item>a MyMosq browser link (<c>https://mymosq.com/mosque/1239</c>)</item>
/// <item>a plain identifier, as it had to be typed in before (<c>assunnah-frechen</c>, <c>1239</c>)</item>
/// </list>
/// </summary>
public static partial class MosqueReferenceParser
{
    /// <summary>Mawaqit's path for its numeric mosque ID; also a valid external ID (it leads to the same page as the slug).</summary>
    public const string MAWAQIT_MOSQUE_ID_PREFIX = "id/";

    /// <summary>
    /// Returns the recognized mosque, or <c>null</c> when <paramref name="input"/> contains neither a known
    /// mosque link nor something that could be a plain identifier.
    /// </summary>
    public static MosqueReference? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        input = input.Trim();

        // The numeric ID first: in the Mawaqit app's share text it is always present, whereas the
        // "Website" line is filled in by the mosque itself and may name another page or be missing.
        Match match = MawaqitMosqueIDRegex().Match(input);
        if (match.Success)
        {
            return new MosqueReference(
                EMosquePrayerTimeProviderType.Mawaqit,
                MAWAQIT_MOSQUE_ID_PREFIX + match.Groups["id"].Value,
                GetMawaqitMosqueName(input));
        }

        match = MawaqitSlugRegex().Match(input);
        if (match.Success)
        {
            return new MosqueReference(
                EMosquePrayerTimeProviderType.Mawaqit,
                match.Groups["slug"].Value,
                GetMawaqitMosqueName(input));
        }

        match = MyMosqMosqueRegex().Match(input);
        if (match.Success)
        {
            return new MosqueReference(EMosquePrayerTimeProviderType.MyMosq, match.Groups["id"].Value);
        }

        match = PlainMawaqitMosqueIDRegex().Match(input);
        if (match.Success)
        {
            return new MosqueReference(EMosquePrayerTimeProviderType.Mawaqit, MAWAQIT_MOSQUE_ID_PREFIX + match.Groups["id"].Value);
        }

        if (PlainIdentifierRegex().IsMatch(input))
        {
            // a plain identifier does not tell which app it belongs to
            return new MosqueReference(EMosquePrayerTimeProviderType.None, input);
        }

        return null;
    }

    private static string? GetMawaqitMosqueName(string input)
    {
        Match match = MawaqitMosqueNameRegex().Match(input);
        return match.Success ? match.Groups["name"].Value : null;
    }

    // e.g. https://mawaqit.net/en/id/11504
    [GeneratedRegex(@"mawaqit\.net/(?:[a-z]{2}(?:[-_][a-z]{2,4})?/)?id/(?<id>\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex MawaqitMosqueIDRegex();

    // e.g. https://mawaqit.net/de/assunnah-frechen, .../de/m/assunnah-frechen (mobile view), .../de/w/assunnah-frechen (widget)
    [GeneratedRegex(@"mawaqit\.net/[a-z]{2}(?:[-_][a-z]{2,4})?/(?:[mw]/)?(?!id(?:[/?#]|$))(?<slug>[a-z0-9][a-z0-9-]*)(?![a-z0-9-])", RegexOptions.IgnoreCase)]
    private static partial Regex MawaqitSlugRegex();

    // first line of the Mawaqit app's share text: "Mosque name - As-Sunnah Moschee Frechen - Frechen";
    // the name also ends at the next "•", because a single-line text field turns the line breaks into spaces
    [GeneratedRegex(@"Mosque name\s*-\s*(?<name>[^\r\n•]*[^\s•])", RegexOptions.IgnoreCase)]
    private static partial Regex MawaqitMosqueNameRegex();

    // e.g. https://mymosq.com/mosque/1239
    [GeneratedRegex(@"mymosq\.com/mosque/(?<id>[a-z0-9_-]+)", RegexOptions.IgnoreCase)]
    private static partial Regex MyMosqMosqueRegex();

    [GeneratedRegex(@"^id/(?<id>\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex PlainMawaqitMosqueIDRegex();

    [GeneratedRegex(@"^[a-z0-9][a-z0-9_-]*$", RegexOptions.IgnoreCase)]
    private static partial Regex PlainIdentifierRegex();
}
