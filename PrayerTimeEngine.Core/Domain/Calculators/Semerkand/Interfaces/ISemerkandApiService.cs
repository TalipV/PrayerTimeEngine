using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.DTOs;
using Refit;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces
{
    // returns the times for the specified time and the subsequent 30 days
    // https://www.semerkandtakvimi.com/Home/CityTimeList?City=32&Year=2024&Day=76
    public interface ISemerkandApiService
    {
        [Post("/countries?language=tr")]
        Task<List<SemerkandCountryResponseDTO>> GetCountries(CancellationToken cancellationToken);

        [Post("/cities?countryID={countryID}")]
        Task<List<SemerkandCityResponseDTO>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);

        [Post("/salaattimes?cityId={cityID}&year={year}")]
        Task<List<SemerkandPrayerTimesResponseDTO>> GetTimesByCityID(int year, int cityID, CancellationToken cancellationToken);
    }
}
