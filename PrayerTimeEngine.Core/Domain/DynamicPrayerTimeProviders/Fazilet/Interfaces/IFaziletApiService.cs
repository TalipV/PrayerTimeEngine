using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Fazilet.Models.DTOs;
using Refit;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Fazilet.Interfaces
{
    public interface IFaziletApiService
    {
        [Get("/daily?districtId=232&lang=1")]
        public Task<FaziletGetCountriesResponseDTO> GetCountries(CancellationToken cancellationToken);

        [Get("/cities-by-country?districtId={countryID}")]
        public Task<List<FaziletCityResponseDTO>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);

        [Get("/daily?districtId={cityID}&lang=2")]
        public Task<FaziletGetTimesByCityIDResponseDTO> GetTimesByCityID(int cityID, CancellationToken cancellationToken);
    }
}
