using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Model;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models
{
    public class MuwaqqitLocationData : BaseLocationData
    {
        public required decimal Longitude { get; init; }
        public required decimal Latitude { get; init; }

        public override ECalculationSource Source => ECalculationSource.Muwaqqit;
    }
}

