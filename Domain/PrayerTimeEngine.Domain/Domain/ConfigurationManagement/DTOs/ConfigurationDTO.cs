namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs;

internal class ConfigurationDTO
{
    public required ICollection<DynamicProfileConfigDTO> DynamicProfileConfigs { get; set; } = [];
    public required ICollection<MosqueProfileConfigDTO> MosqueProfileConfigs { get; set; } = [];
}
