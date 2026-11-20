using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Services;

// Returns list of mosques for specific location search text with all the mosque data but only today's times
// https://mawaqit.net/api/2.0/mosque/search?word=ABCDE&fields=slug%2Clabel

public partial class MawaqitApiService(
        HttpClient httpClient
    ) : IMawaqitApiService
{
    public async Task<MawaqitResponseDTO> GetPrayerTimesAsync(string externalID, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await httpClient.GetAsync(externalID, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new Exception($"{externalID} not found");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to fetch data for {externalID}");
        }

        string pageContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = new HtmlDocument();
        doc.LoadHtml(pageContent);

        // the declaration keyword changes over time (Mawaqit switched from "var" to "let" in October 2026)
        HtmlNode scriptNode = doc.DocumentNode.SelectSingleNode("//script[contains(text(), 'confData = ')]")
            ?? throw new Exception($"Script containing confData not found for {externalID}");

        // fix
        Match match =
            confDataExtractionRegex().Match(input: scriptNode.InnerText);

        if (!match.Success)
        {
            throw new Exception($"Failed to extract confData JSON for {externalID}");
        }

        return JsonSerializer.Deserialize<MawaqitResponseDTO>(match.Groups[1].Value);
    }

    [GeneratedRegex(@"(?:var|let|const) confData = (.*?);", RegexOptions.Singleline)]
    private static partial Regex confDataExtractionRegex();

    public async Task<bool> ValidateData(string externalID, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await httpClient.GetAsync(externalID, cancellationToken);
        return response.StatusCode != System.Net.HttpStatusCode.NotFound;
    }

    public async Task<string?> GetSlugAsync(string externalID, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await httpClient.GetAsync(externalID, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to fetch data for {externalID}");
        }

        string pageContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = new HtmlDocument();
        doc.LoadHtml(pageContent);

        // every mosque page (also the one under "id/11504") names its slug page as the canonical one,
        // e.g. <link rel="canonical" href="https://mawaqit.net/de/assunnah-frechen" />
        string? canonicalUrl = doc.DocumentNode
            .SelectSingleNode("//link[@rel='canonical']")
            ?.GetAttributeValue("href", null);

        if (Uri.TryCreate(canonicalUrl, UriKind.Absolute, out Uri? canonicalUri)
            && canonicalUri.Segments.Length > 0)
        {
            string slug = canonicalUri.Segments[^1].Trim('/');

            if (slug.Length > 0 && !int.TryParse(slug, out _))
            {
                return slug;
            }
        }

        // without a usable canonical link, a slug input is still fine as it is; a numeric ID is not
        return externalID.StartsWith("id/", StringComparison.OrdinalIgnoreCase)
            ? throw new Exception($"The slug of the mosque {externalID} could not be determined")
            : externalID;
    }
}