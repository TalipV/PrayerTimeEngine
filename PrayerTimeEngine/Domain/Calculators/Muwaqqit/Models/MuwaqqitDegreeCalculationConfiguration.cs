using PrayerTimeEngine.Common.Enum;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models
{
    public class MuwaqqitDegreeCalculationConfiguration : MuwaqqitCalculationConfiguration
    {
        public MuwaqqitDegreeCalculationConfiguration(
            ETimeType timeType, int minuteAdjustment, double degree, bool isTimeShown = true) : base(timeType, minuteAdjustment, isTimeShown)
        {
            Degree = degree;
        }

        public double Degree { get; }
    }
}
