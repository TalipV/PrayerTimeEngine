using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;

namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs;

/// <summary>
/// Configuration DTO for <see cref="ProfileManagement.Models.Entities.MosqueProfile"/>
/// </summary>
public class MosqueProfileConfigDTO : ProfileConfigDTO
{
    public required EMosquePrayerTimeProviderType MosqueProviderType { get; set; }

    public required string ExternalID { get; set; }
}
