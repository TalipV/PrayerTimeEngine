using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.DTOs
{
    public class SemerkandCountryResponseDTO
    {
        [JsonPropertyName("Id")]
        public int ID { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("DisplayName")]
        public string DisplayName { get; set; }
    }
}
