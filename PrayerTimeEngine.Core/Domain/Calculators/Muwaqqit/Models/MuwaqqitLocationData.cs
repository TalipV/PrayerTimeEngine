using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models
{
    public class MuwaqqitLocationData : BaseLocationData
    {
        public required decimal Longitude { get; init; }
        public required decimal Latitude { get; init; }
        public required string TimezoneName { get; set; }
        public override ECalculationSource Source => ECalculationSource.Muwaqqit;

        public override bool Equals(object obj)
        {
            if (obj is not MuwaqqitLocationData otherLocationData)
                return false;

            return this.Longitude == otherLocationData.Longitude
                && this.Latitude == otherLocationData.Latitude
                && this.TimezoneName == otherLocationData.TimezoneName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Longitude, Latitude, TimezoneName);
        }
    }
}

