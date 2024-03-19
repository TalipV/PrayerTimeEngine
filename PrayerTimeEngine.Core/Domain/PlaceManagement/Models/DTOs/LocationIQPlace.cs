using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Models.DTOs
{
    public class LocationIQPlace
    {
        [JsonPropertyName("place_id")]
        public string PlaceID { get; set; }

        [JsonPropertyName("osm_type")]
        public string OsmType { get; set; }

        [JsonPropertyName("osm_id")]
        public string OsmID { get; set; }

        [JsonPropertyName("lat")]
        public string Latitude { get; set; }

        [JsonPropertyName("lon")]
        public string Longitude { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("address")]
        public LocationIQAddress Address { get; set; }
    }
}
