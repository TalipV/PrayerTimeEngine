using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models
{
    public class MuwaqqitLocationData : BaseLocationData
    {
        public required decimal Longitude { get; init; }
        public required decimal Latitude { get; init; }
        public required string TimezoneName { get; set; }
        public override EDynamicPrayerTimeProviderType Source => EDynamicPrayerTimeProviderType.Muwaqqit;

        public override bool Equals(object obj)
        {
            if (obj is not MuwaqqitLocationData otherLocationData)
                return false;

            return Longitude == otherLocationData.Longitude
                && Latitude == otherLocationData.Latitude
                && TimezoneName == otherLocationData.TimezoneName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Longitude, Latitude, TimezoneName);
        }

        public override string ToString()
        {
            return $"{Source}: {TimezoneName}, {Latitude}°/{Longitude}°";
        }
    }
}

