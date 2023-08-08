using System;
using PrayerTimeEngine.Domain.Model;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models
{
	public class MuwaqqitLocationInfo : ILocationInfo
    {
        public required decimal Longitude { get; init; }
        public required decimal Latitude { get; init; }
    }
}

