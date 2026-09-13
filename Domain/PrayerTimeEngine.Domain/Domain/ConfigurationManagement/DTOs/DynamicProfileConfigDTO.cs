namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs;

/// <summary>
/// Configuration DTO for <see cref="ProfileManagement.Models.Entities.DynamicProfile"/>
/// </summary>
public class DynamicProfileConfigDTO : ProfileConfigDTO
{
    public required PlaceInfoDTO PlaceInfo { get; set; }
    public required ICollection<TimeConfigDTO> TimeConfigs { get; set; }
    public required ICollection<LocationConfigDTO> LocationConfigs { get; set; }
}