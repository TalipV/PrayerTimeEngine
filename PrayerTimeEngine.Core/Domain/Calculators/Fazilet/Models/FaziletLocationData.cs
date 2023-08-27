using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Model;

namespace PrayerTimeEngine.Domain.Calculators.Fazilet.Models
{
    public class FaziletLocationData : BaseLocationData
    {
        public required string CountryName { get; init; }
        public required string CityName { get; init; }

        public override ECalculationSource Source => ECalculationSource.Fazilet;
    }
}