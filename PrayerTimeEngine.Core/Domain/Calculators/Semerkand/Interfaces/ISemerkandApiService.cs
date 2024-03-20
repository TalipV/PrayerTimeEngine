using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.DTOs;
using Refit;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces
{
    public interface ISemerkandApiService
    {
        [Post("/countries?language=tr")]
        Task<List<SemerkandCountryResponseDTO>> GetCountries(CancellationToken cancellationToken);

        [Post("/cities?countryID={countryID}")]
        Task<List<SemerkandCityResponseDTO>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);

        [Post("/salaattimes?cityId={cityID}&year={year}")]
        Task<List<SemerkandPrayerTimesResponseDTO>> GetTimesByCityID(int cityID, int year, CancellationToken cancellationToken);
    }
}
