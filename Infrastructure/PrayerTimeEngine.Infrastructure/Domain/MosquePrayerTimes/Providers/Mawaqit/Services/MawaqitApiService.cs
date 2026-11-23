using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using PrayerTimeEngine.Core.Common.Http;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;
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
        HttpResponseMessage response = await GetPageAsync(externalID, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw ExternalServiceErrors.FromResponse(SERVICE_NAME, response, throttle: null);
        }

        string pageContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = new HtmlDocument();
        doc.LoadHtml(pageContent);

        // the declaration keyword changes over time (Mawaqit switched from "var" to "let" in October 2026)
        HtmlNode scriptNode = doc.DocumentNode.SelectSingleNode("//script[contains(text(), 'confData = ')]")
            ?? throw new ExternalServiceException(SERVICE_NAME, EExternalServiceFailure.InvalidResponse, details: $"no prayer time data (confData) on the page of {externalID}");

        // fix
        Match match =
            confDataExtractionRegex().Match(input: scriptNode.InnerText);

        if (!match.Success)
        {
            throw new ExternalServiceException(SERVICE_NAME, EExternalServiceFailure.InvalidResponse, details: $"prayer time data (confData) of {externalID} not readable");
        }

        return JsonSerializer.Deserialize<MawaqitResponseDTO>(match.Groups[1].Value);
    }

    private const string SERVICE_NAME = "Mawaqit";

    /// <summary>The page of the mosque; failures without a response are classified like those of the other services.</summary>
    private async Task<HttpResponseMessage> GetPageAsync(string externalID, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetAsync(externalID, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (ExternalServiceErrors.FromTransportException(SERVICE_NAME, exception, cancellationToken) is ExternalServiceException externalServiceException)
        {
            throw externalServiceException;
        }
    }

    [GeneratedRegex(@"(?:var|let|const) confData = (.*?);", RegexOptions.Singleline)]
    private static partial Regex confDataExtractionRegex();

    public async Task<bool> ValidateData(string externalID, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await GetPageAsync(externalID, cancellationToken).ConfigureAwait(false);
        return response.StatusCode != System.Net.HttpStatusCode.NotFound;
    }

    public async Task<MosqueIdentity?> GetMosqueIdentityAsync(string externalID, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await GetPageAsync(externalID, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw ExternalServiceErrors.FromResponse(SERVICE_NAME, response, throttle: null);
        }

        string pageContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = new HtmlDocument();
        doc.LoadHtml(pageContent);

        return new MosqueIdentity(GetSlug(doc, externalID), GetMosqueName(doc));
    }

    private static string GetSlug(HtmlDocument doc, string externalID)
    {
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

    /// <summary>
    /// The mosque's name from the page data (<c>confData.name</c>, e.g. "As-Sunnah Moschee Frechen - Frechen"),
    /// which is the same in every language of the page; <c>null</c> when the page does not contain it.
    /// </summary>
    private static string? GetMosqueName(HtmlDocument doc)
    {
        HtmlNode? scriptNode = doc.DocumentNode.SelectSingleNode("//script[contains(text(), 'confData = ')]");
        if (scriptNode is null)
        {
            return null;
        }

        Match match = confDataExtractionRegex().Match(scriptNode.InnerText);
        if (!match.Success)
        {
            return null;
        }

        try
        {
            using var confData = JsonDocument.Parse(match.Groups[1].Value);

            return confData.RootElement.TryGetProperty("name", out JsonElement nameElement)
                && nameElement.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(nameElement.GetString())
                    ? nameElement.GetString()!.Trim()
                    : null;
        }
        catch (JsonException)
        {
            // the name is only a nicer profile name; without it the profile is named after the identifier
            return null;
        }
    }
}