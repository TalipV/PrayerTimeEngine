using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces
{
    public interface IFaziletApiService
    {
        public Task<Dictionary<string, int>> GetCountries(CancellationToken cancellationToken);
        public Task<Dictionary<string, int>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);
        public Task<List<FaziletPrayerTimes>> GetTimesByCityID(int cityID, CancellationToken cancellationToken);
    }
}
