namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models;

public class MuwaqqitDegreeCalculationConfiguration : MuwaqqitCalculationConfiguration
{
    public required double Degree { get; init; }

    public override bool Equals(object obj)
    {
        if (obj is not MuwaqqitDegreeCalculationConfiguration otherSettingConfig)
            return false;

        return base.Equals(otherSettingConfig)
            && Degree == otherSettingConfig.Degree;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Degree);
    }

    public override string ToString()
    {
        return $"{base.ToString()}, Degree: {Degree}°";
    }
}
