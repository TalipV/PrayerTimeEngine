using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Semerkand.Models
{
    public class SemerkandLocationData : BaseLocationData
    {
        public required string CountryName { get; init; }
        public required string CityName { get; init; }
        public required string TimezoneName { get; set; }
        public override EDynamicPrayerTimeProviderType Source => EDynamicPrayerTimeProviderType.Semerkand;

        public override bool Equals(object obj)
        {
            if (obj is not SemerkandLocationData otherLocationData)
                return false;

            return CountryName == otherLocationData.CountryName
                && CityName == otherLocationData.CityName
                && TimezoneName == otherLocationData.TimezoneName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CountryName, CityName, TimezoneName);
        }

        public override string ToString()
        {
            return $"{Source}: {CountryName}, {CityName}, Timezone: {TimezoneName}";
        }
    }
}

