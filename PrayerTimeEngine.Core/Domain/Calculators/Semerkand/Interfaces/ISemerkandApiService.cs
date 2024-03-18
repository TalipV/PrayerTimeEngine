using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces
{
    public interface ISemerkandApiService
    {
        Task<List<SemerkandCountryResponseDTO>> GetCountries(CancellationToken cancellationToken);
        Task<List<SemerkandCityResponseDTO>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);
        Task<List<SemerkandPrayerTimesResponseDTO>> GetTimesByCityID(LocalDate date, int cityID, CancellationToken cancellationToken);
    }
}
