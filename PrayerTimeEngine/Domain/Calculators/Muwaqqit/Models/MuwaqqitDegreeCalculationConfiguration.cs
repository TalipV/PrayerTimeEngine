using PrayerTimeEngine.Common.Enum;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models
{
    public class MuwaqqitDegreeCalculationConfiguration : MuwaqqitCalculationConfiguration
    {
        public static readonly Dictionary<ETimeType, double> DegreePrayerTimeEvents =
            new Dictionary<ETimeType, double>
            {
                [ETimeType.FajrStart] = -12.0,
                [ETimeType.FajrGhalas] = -7.0,
                [ETimeType.FajrKaraha] = -3.0,

                [ETimeType.DuhaStart] = 5.0,
                [ETimeType.AsrKaraha] = 5.0,

                [ETimeType.MaghribIshtibaq] = -10.0,
                [ETimeType.MaghribEnd] = -12.0,

                [ETimeType.IshaStart] = -15.0,
                [ETimeType.IshaEnd] = -15.0,
            };

        public MuwaqqitDegreeCalculationConfiguration(
            int minuteAdjustment, double degree, bool isTimeShown = true) : base(minuteAdjustment, isTimeShown)
        {
            Degree = degree;
        }

        public double Degree { get; }
    }
}
