using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models
{
    public class MuwaqqitLocationData : BaseLocationData
    {
        public required decimal Longitude { get; init; }
        public required decimal Latitude { get; init; }

        public override ECalculationSource Source => ECalculationSource.Muwaqqit;
    }
}

