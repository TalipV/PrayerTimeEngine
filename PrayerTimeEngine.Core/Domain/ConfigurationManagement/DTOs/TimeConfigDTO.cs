using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs;

/// <summary>
/// Configuration DTO for <see cref="ProfileManagement.Models.Entities.ProfileTimeConfig"/>
/// </summary>
public class TimeConfigDTO
{
    [JsonIgnore]
    public required ETimeType TimeType { get; set; }

    [JsonPropertyName(nameof(TimeType))]
    public string TimeTypeString
    {
        get
        {
            return TimeType.ToString();
        }
        set
        {
            TimeType = Enum.Parse<ETimeType>(value);
        }
    }

    public required GenericSettingConfiguration CalculationConfiguration { get; set; }
}