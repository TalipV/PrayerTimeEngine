using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models
{
    public class FaziletLocationData : BaseLocationData
    {
        public required string CountryName { get; init; }
        public required string CityName { get; init; }

        public override ECalculationSource Source => ECalculationSource.Fazilet;

        public override bool Equals(object obj)
        {
            if (obj is not FaziletLocationData otherLocationData)
                return false;

            return this.CountryName == otherLocationData.CountryName
                && this.CityName == otherLocationData.CityName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CountryName, CityName);
        }

        public override string ToString()
        {
            return $"{Source}: {CountryName}, {CityName}";
        }
    }
}