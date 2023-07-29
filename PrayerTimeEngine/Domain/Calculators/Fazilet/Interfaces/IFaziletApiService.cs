using PrayerTimeEngine.Domain.Calculators.Fazilet.Models;

namespace PrayerTimeEngine.Domain.Calculators.Fazilet.Interfaces
{
    public interface IFaziletApiService
    {
        public Task<Dictionary<string, int>> GetCountries();
        public Task<Dictionary<string, int>> GetCitiesByCountryID(int countryID);
        public Task<List<FaziletPrayerTimes>> GetTimesByCityID(int cityID);
    }
}
