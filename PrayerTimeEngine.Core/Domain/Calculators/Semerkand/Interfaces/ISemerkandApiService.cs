using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces
{
    public interface ISemerkandApiService
    {
        public Task<Dictionary<string, int>> GetCountries(CancellationToken cancellationToken);
        public Task<Dictionary<string, int>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);
        public Task<List<SemerkandPrayerTimes>> GetTimesByCityID(LocalDate date, string timezone, int cityID, CancellationToken cancellationToken);
    }
}
