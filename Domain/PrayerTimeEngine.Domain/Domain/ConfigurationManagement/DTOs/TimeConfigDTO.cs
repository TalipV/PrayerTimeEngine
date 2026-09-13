using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs;

/// <summary>
/// Configuration DTO for <see cref="ProfileManagement.Models.Entities.ProfileTimeConfig"/>
/// </summary>
public class TimeConfigDTO
{
    public required ETimeType TimeType { get; set; }

    public required GenericSettingConfiguration CalculationConfiguration { get; set; }
}