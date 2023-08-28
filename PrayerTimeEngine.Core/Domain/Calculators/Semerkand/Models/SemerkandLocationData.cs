using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models
{
    public class SemerkandLocationData : BaseLocationData
    {
        public required string CountryName { get; init; }
        public required string CityName { get; init; }

        public override ECalculationSource Source => ECalculationSource.Semerkand;
    }
}

