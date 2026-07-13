using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.DTOs;
using Refit;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces;

// examples:
// - https://fazilettakvimi.com/api/cms/daily?districtId=232&lang=1
// - https://fazilettakvimi.com/api/cms/cities-by-country?districtId=1
// - https://fazilettakvimi.com/api/cms/daily?lang=2&districtId=1

public interface IFaziletApiService
{
    [Get("/daily?districtId=232&lang=1")]
    public Task<FaziletGetCountriesResponseDTO> GetCountries(CancellationToken cancellationToken);

    [Get("/cities-by-country")]
    public Task<List<FaziletCityResponseDTO>> GetCitiesByCountryID([AliasAs("districtId")] int countryID, CancellationToken cancellationToken);

    [Get("/daily?lang=2")]
    public Task<FaziletGetTimesByCityIDResponseDTO> GetTimesByCityID([AliasAs("districtId")] int cityID, CancellationToken cancellationToken);
}
