namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs;

/// <summary>
/// Configuration DTO for <see cref="PlaceManagement.Models.ProfilePlaceInfo"/>
/// </summary>
public class PlaceInfoDTO
{
    public string ExternalID { get; set; }

    public required decimal Longitude { get; set; }
    public required decimal Latitude { get; set; }
    public required string InfoLanguageCode { get; set; }

    public required string Country { get; set; }
    public string State { get; set; }
    public required string City { get; set; }
    public required string CityDistrict { get; set; }
    public required string PostCode { get; set; }
    public required string Street { get; set; }

    public required string TimezoneDisplayName { get; set; }
    public required string TimezoneName { get; set; }
    public required int TimezoneUtcOffsetSeconds { get; set; }
}
