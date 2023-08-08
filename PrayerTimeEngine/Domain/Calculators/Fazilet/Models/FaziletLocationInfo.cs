using System;
using PrayerTimeEngine.Domain.Model;

namespace PrayerTimeEngine.Domain.Calculators.Fazilet.Models
{
    public class FaziletLocationInfo : ILocationInfo
    {
        public required string CountryName { get; init; }
        public required string CityName { get; init; }
    }
}