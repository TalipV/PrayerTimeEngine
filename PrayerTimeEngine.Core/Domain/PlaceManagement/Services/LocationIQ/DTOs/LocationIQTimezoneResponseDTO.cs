using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ.DTOs;

public class LocationIQTimezoneResponseDTO
{
    [JsonPropertyName("timezone")]
    public LocationIQTimezone LocationIQTimezone { get; set; }
}

public class LocationIQTimezone
{
    [JsonPropertyName("short_name")]
    public string ShortName { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("offset_sec")]
    public int OffsetSeconds { get; set; }
}
