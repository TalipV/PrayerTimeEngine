using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.DTOs;
using Refit;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services;

// Refit twin: the HTTP-specific contract. This is an Infrastructure implementation detail
// and is never seen by the inner rings.
internal interface IFaziletApiClient
{
    [Get("/daily?districtId=232&lang=1")]
    Task<FaziletGetCountriesResponseDTO> GetCountries(CancellationToken cancellationToken);

    [Get("/cities-by-country")]
    Task<List<FaziletCityResponseDTO>> GetCitiesByCountryID([AliasAs("districtId")] int countryID, CancellationToken cancellationToken);

    [Get("/daily?lang=2")]
    Task<FaziletGetTimesByCityIDResponseDTO> GetTimesByCityID([AliasAs("districtId")] int cityID, CancellationToken cancellationToken);
}

// Adapter: implements the Domain port by delegating to the Refit client.
internal sealed class FaziletApiService(IFaziletApiClient client) : IFaziletApiService
{
    public Task<FaziletGetCountriesResponseDTO> GetCountries(CancellationToken cancellationToken)
        => client.GetCountries(cancellationToken);

    public Task<List<FaziletCityResponseDTO>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken)
        => client.GetCitiesByCountryID(countryID, cancellationToken);

    public Task<FaziletGetTimesByCityIDResponseDTO> GetTimesByCityID(int cityID, CancellationToken cancellationToken)
        => client.GetTimesByCityID(cityID, cancellationToken);
}
