using System;
using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Model;

namespace PrayerTimeEngine.Domain.Calculators.Semerkand.Models
{
	public class SemerkandLocationData : BaseLocationData
	{
		public required string CountryName { get; init; }
		public required string CityName { get; init; }

        public override ECalculationSource Source => ECalculationSource.Semerkand;
    }
}

