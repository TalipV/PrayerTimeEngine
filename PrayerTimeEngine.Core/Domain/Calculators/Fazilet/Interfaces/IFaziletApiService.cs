using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces
{
    public interface IFaziletApiService
    {
        public Task<FaziletGetCountriesResponseDTO> GetCountries(CancellationToken cancellationToken);
        public Task<List<FaziletCityResponseDTO>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);
        public Task<FaziletGetTimesByCityIDResponseDTO> GetTimesByCityID(int cityID, CancellationToken cancellationToken);
    }
}
