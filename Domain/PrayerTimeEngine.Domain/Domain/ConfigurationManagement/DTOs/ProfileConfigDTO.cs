namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs;

/// <summary>
/// Configuration DTO for <see cref="ProfileManagement.Models.Entities.Profile"/>
/// </summary>
public abstract class ProfileConfigDTO
{
    public required string Name { get; set; }
    public required int SequenceNo { get; set; }
}