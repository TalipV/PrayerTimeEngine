using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs;

/// <summary>
/// Configuration DTO for <see cref="ProfileManagement.Models.Entities.ProfileLocationConfig"/>
/// </summary>
public class LocationConfigDTO
{
    public required EDynamicPrayerTimeProviderType ProviderType { get; set; }

    public required BaseLocationData LocationData { get; set; }
}
