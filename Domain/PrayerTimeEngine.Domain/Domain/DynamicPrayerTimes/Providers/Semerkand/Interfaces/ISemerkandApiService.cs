using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;

// ONION port: technology-free contract. The concrete HTTP routing (Refit) lives in the
// Infrastructure adapter, so the Application ring depends only on this abstraction.
public interface ISemerkandApiService
{
    Task<List<SemerkandCountryResponseDTO>> GetCountries(CancellationToken cancellationToken);

    Task<List<SemerkandCityResponseDTO>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);

    Task<List<SemerkandPrayerTimesResponseDTO>> GetTimesByCityID(int year, int cityID, CancellationToken cancellationToken);
}
