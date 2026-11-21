using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces;

// ONION port: technology-free contract. The concrete HTTP routing (Refit) lives in the
// Infrastructure adapter, so the Application ring depends only on this abstraction.
public interface IFaziletApiService
{
    Task<FaziletGetCountriesResponseDTO> GetCountries(CancellationToken cancellationToken);

    Task<List<FaziletCityResponseDTO>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);

    /// <summary>
    /// The prayer times of <paramref name="date"/> together with those of the day before and the day after.
    /// Days outside of the range the API has data for come back with empty times.
    /// </summary>
    Task<FaziletGetTimesByCityIDResponseDTO> GetTimesByCityID(int cityID, LocalDate date, CancellationToken cancellationToken);
}
