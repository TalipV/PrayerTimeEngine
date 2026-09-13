using HtmlAgilityPack;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.DTOs;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Services;

// Returns list of mosques for specific location search text with all the mosque data but only today's times
// https://mawaqit.net/api/2.0/mosque/search?word=ABCDE&fields=slug%2Clabel

public partial class MawaqitApiService(
        HttpClient httpClient
    ) : IMawaqitApiService
{
    public async Task<MawaqitResponseDTO> GetPrayerTimesAsync(string externalID, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(externalID, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            throw new Exception($"{externalID} not found");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to fetch data for {externalID}");

        var pageContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = new HtmlDocument();
        doc.LoadHtml(pageContent);

        var scriptNode = doc.DocumentNode.SelectSingleNode("//script[contains(text(), 'var confData = ')]")
            ?? throw new Exception($"Script containing confData not found for {externalID}");

        // fix
        var match =
            confDataExtractionRegex().Match(input: scriptNode.InnerText);

        if (!match.Success)
            throw new Exception($"Failed to extract confData JSON for {externalID}");

        return JsonSerializer.Deserialize<MawaqitResponseDTO>(match.Groups[1].Value);
    }

    [GeneratedRegex(@"var confData = (.*?);", RegexOptions.Singleline)]
    private static partial Regex confDataExtractionRegex();

    public async Task<bool> ValidateData(string externalID, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(externalID, cancellationToken);
        return response.StatusCode != System.Net.HttpStatusCode.NotFound;
    }
}