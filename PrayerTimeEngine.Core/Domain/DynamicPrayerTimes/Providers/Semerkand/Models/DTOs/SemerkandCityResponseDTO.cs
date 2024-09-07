using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.DTOs;

public class SemerkandCityResponseDTO
{
    [JsonPropertyName("Id")]
    public int ID { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("DisplayName")]
    public string DisplayName { get; set; }
}
