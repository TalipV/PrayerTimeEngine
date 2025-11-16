using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs;

/// <summary>
/// Configuration DTO for <see cref="ProfileManagement.Models.Entities.ProfileLocationConfig"/>
/// </summary>
public class LocationConfigDTO
{
    [JsonIgnore]
    public required EDynamicPrayerTimeProviderType ProviderType { get; set; }

    [JsonPropertyName(nameof(ProviderType))]
    public string ProviderTypeString
    {
        get
        {
            return ProviderType.ToString();
        }
        set
        {
            ProviderType = Enum.Parse<EDynamicPrayerTimeProviderType>(value);
        }
    }

    public required BaseLocationData LocationData { get; set; }
}
