using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.DTOs;
using Refit;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services;

// Refit twin: the HTTP-specific contract. Infrastructure implementation detail.
internal interface ISemerkandApiClient
{
    [Get("/countries")]
    Task<List<SemerkandCountryResponseDTO>> GetCountries(CancellationToken cancellationToken);

    [Get("/countries/{countryID}/cities")]
    Task<List<SemerkandCityResponseDTO>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);

    [Get("/salaat-times")]
    Task<List<SemerkandPrayerTimesResponseDTO>> GetTimesByCityID([AliasAs("year")] int year, [AliasAs("cityId")] int cityID, CancellationToken cancellationToken);
}

// Adapter: implements the Domain port by delegating to the Refit client.
internal sealed class SemerkandApiService(ISemerkandApiClient client) : ISemerkandApiService
{
    public Task<List<SemerkandCountryResponseDTO>> GetCountries(CancellationToken cancellationToken)
        => client.GetCountries(cancellationToken);

    public Task<List<SemerkandCityResponseDTO>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken)
        => client.GetCitiesByCountryID(countryID, cancellationToken);

    public Task<List<SemerkandPrayerTimesResponseDTO>> GetTimesByCityID(int year, int cityID, CancellationToken cancellationToken)
        => client.GetTimesByCityID(year, cityID, cancellationToken);
}
