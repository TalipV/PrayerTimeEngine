using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.DTOs;

/// <summary>
/// Body of the request for the prayer times of one month. Without <see cref="Overrides"/> the
/// backend uses the settings of the base calculation <see cref="Method"/>.
/// </summary>
public class WaqtiMonthPrayerTimesRequestDTO
{
    [JsonPropertyName("latitude")]
    public required decimal Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public required decimal Longitude { get; init; }

    [JsonPropertyName("elevation")]
    public required int Elevation { get; init; }

    [JsonPropertyName("timezone")]
    public required string Timezone { get; init; }

    [JsonPropertyName("year")]
    public required int Year { get; init; }

    [JsonPropertyName("month")]
    public required int Month { get; init; }

    [JsonPropertyName("method")]
    public required string Method { get; init; }

    [JsonPropertyName("overrides")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WaqtiOverridesRequestDTO? Overrides { get; init; }
}

/// <summary>
/// Settings which replace the ones of the base calculation method. Only the sun angles are used.
/// </summary>
public class WaqtiOverridesRequestDTO
{
    /// <summary>
    /// Depression of the sun below the horizon for Fajr in degrees (negative: above the horizon).
    /// </summary>
    [JsonPropertyName("fajrAngle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? FajrAngle { get; init; }

    /// <summary>
    /// Depression of the sun below the horizon for Isha in degrees (negative: above the horizon).
    /// </summary>
    [JsonPropertyName("ishaaAngle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? IshaaAngle { get; init; }
}
