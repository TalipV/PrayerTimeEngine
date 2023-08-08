using System;
using PrayerTimeEngine.Domain.Model;

namespace PrayerTimeEngine.Domain.Calculators.Semerkand.Models
{
	public class SemerkandLocationInfo : ILocationInfo
	{
		public required string CountryName { get; init; }
		public required string CityName { get; init; }
    }
}

