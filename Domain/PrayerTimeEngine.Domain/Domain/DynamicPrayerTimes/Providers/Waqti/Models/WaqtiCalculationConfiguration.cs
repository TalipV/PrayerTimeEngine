using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models;

/// <summary>
/// Configuration of a time which is calculated by Waqti with a specific base calculation method
/// (see <see cref="WaqtiMethods"/>).
/// </summary>
/// <remarks>
/// Waqti has no own value for the times in <see cref="DEGREE_TIME_TYPES"/>. Like with Muwaqqit,
/// they are taken from another time calculated for the sun angle in <see cref="Degree"/>
/// (negative: below the horizon, positive: above it, the same convention as Muwaqqit).
/// </remarks>
public class WaqtiCalculationConfiguration : GenericSettingConfiguration
{
    /// <summary>
    /// Times taken from Fajr (morning) respectively Isha (evening) calculated for <see cref="Degree"/>.
    /// </summary>
    public static readonly IReadOnlySet<ETimeType> FAJR_DEGREE_TIME_TYPES = new HashSet<ETimeType> { ETimeType.FajrGhalas, ETimeType.FajrKaraha };
    public static readonly IReadOnlySet<ETimeType> ISHA_DEGREE_TIME_TYPES = new HashSet<ETimeType> { ETimeType.AsrKaraha, ETimeType.MaghribIshtibak };
    public static readonly IReadOnlySet<ETimeType> DEGREE_TIME_TYPES = new HashSet<ETimeType>([.. FAJR_DEGREE_TIME_TYPES, .. ISHA_DEGREE_TIME_TYPES]);

    public override EDynamicPrayerTimeProviderType Source => EDynamicPrayerTimeProviderType.Waqti;

    public required string Method { get; init; }

    /// <summary>
    /// Sun angle for the times in <see cref="DEGREE_TIME_TYPES"/>, ignored for all other times.
    /// </summary>
    public double? Degree { get; init; }

    #region System.Object overrides

    public override bool Equals(object? obj)
    {
        if (obj is not WaqtiCalculationConfiguration otherSettingConfig)
        {
            return false;
        }

        return base.Equals(otherSettingConfig)
            && Method == otherSettingConfig.Method
            && Degree == otherSettingConfig.Degree;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Method, Degree);
    }

    public override string ToString()
    {
        return Degree is double degree
            ? $"{base.ToString()}, Method: {Method}, Degree: {degree}°"
            : $"{base.ToString()}, Method: {Method}";
    }

    #endregion System.Object overrides
}
