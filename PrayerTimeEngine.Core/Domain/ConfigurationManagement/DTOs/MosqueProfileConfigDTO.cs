using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs;

/// <summary>
/// Configuration DTO for <see cref="ProfileManagement.Models.Entities.MosqueProfile"/>
/// </summary>
public class MosqueProfileConfigDTO : ProfileConfigDTO
{
    [JsonIgnore]
    public required EMosquePrayerTimeProviderType MosqueProviderType { get; set; }

    [JsonPropertyName(nameof(MosqueProviderType))]
    public string MosqueProviderTypeString
    {
        get
        {
            return MosqueProviderType.ToString();
        }
        set
        {
            MosqueProviderType = Enum.Parse<EMosquePrayerTimeProviderType>(value);
        }
    }

    public required string ExternalID { get; set; }
}
