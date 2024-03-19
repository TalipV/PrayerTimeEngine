using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.DTOs
{
    public class FaziletCityResponseDTO
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("adi")]
        public string Name { get; set; }
    }
}
