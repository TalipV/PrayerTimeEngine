using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.DTOs
{
    public class FaziletGetCountriesResponseDTO
    {
        [JsonPropertyName("ulkeler")]
        public List<FaziletCountryResponseDTO> Countries { get; set; }
    }

    public class FaziletCountryResponseDTO
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("adi")]
        public string Name { get; set; }
    }
}
