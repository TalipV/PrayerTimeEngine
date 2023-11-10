namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models
{
    public class MuwaqqitDegreeCalculationConfiguration : MuwaqqitCalculationConfiguration
    {
        public required double Degree { get; init; }

        public override bool Equals(object obj)
        {
            if (obj is not MuwaqqitDegreeCalculationConfiguration otherSettingConfig)
                return false;

            return base.Equals(otherSettingConfig)
                && this.Degree == otherSettingConfig.Degree;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Degree);
        }
    }
}
