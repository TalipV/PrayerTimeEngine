using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ.DTOs
{
    public class LocationIQAddress
    {
        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("monitoring_station")]
        public string MonitoringStation { get; set; }

        [JsonPropertyName("road")]
        public string Road { get; set; }

        [JsonPropertyName("neighbourhood")]
        public string Neighbourhood { get; set; }

        [JsonPropertyName("suburb")]
        public string Suburb { get; set; }

        [JsonPropertyName("postcode")]
        public string Postcode { get; set; }

        [JsonPropertyName("house_number")]
        public string HouseNumber { get; set; }
    }
}
