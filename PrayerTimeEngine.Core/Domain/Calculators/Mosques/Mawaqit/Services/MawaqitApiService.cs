﻿using HtmlAgilityPack;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Models.DTOs;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Services
{
    public class MawaqitApiService(
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

            var match =
                Regex.Match(
                    input: scriptNode.InnerText,
                    pattern: @"var confData = (.*?);",
                    options: RegexOptions.Singleline);

            if (!match.Success)
                throw new Exception($"Failed to extract confData JSON for {externalID}");

            return JsonSerializer.Deserialize<MawaqitResponseDTO>(match.Groups[1].Value);
        }
    }
}
