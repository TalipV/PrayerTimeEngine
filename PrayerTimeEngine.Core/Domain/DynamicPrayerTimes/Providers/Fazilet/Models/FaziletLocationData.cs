using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models
{
    public class FaziletLocationData : BaseLocationData
    {
        public required string CountryName { get; init; }
        public required string CityName { get; init; }

        public override EDynamicPrayerTimeProviderType Source => EDynamicPrayerTimeProviderType.Fazilet;

        public override bool Equals(object obj)
        {
            if (obj is not FaziletLocationData otherLocationData)
                return false;

            return CountryName == otherLocationData.CountryName
                && CityName == otherLocationData.CityName;
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