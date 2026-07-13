using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.DTOs;
using Refit;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;

// examples:
// - https://semerkandtakvimi.com/api/countries
// - https://semerkandtakvimi.com/api/countries/1/cities
// - https://semerkandtakvimi.com/api/cities/34/districts
// - https://semerkandtakvimi.com/api/salaat-times?year=2026&CityId=83

// returns the times for the specified time and the subsequent 30 days
// https://www.semerkandtakvimi.com/Home/CityTimeList?City=32&Year=2024&Day=76

public interface ISemerkandApiService
{
    [Get("/countries")]
    Task<List<SemerkandCountryResponseDTO>> GetCountries(CancellationToken cancellationToken);

    [Get("/countries/{countryID}/cities")]
    Task<List<SemerkandCityResponseDTO>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);

    [Get("/salaat-times")]
    Task<List<SemerkandPrayerTimesResponseDTO>> GetTimesByCityID([AliasAs("year")] int year, [AliasAs("cityId")] int cityID, CancellationToken cancellationToken);
}
