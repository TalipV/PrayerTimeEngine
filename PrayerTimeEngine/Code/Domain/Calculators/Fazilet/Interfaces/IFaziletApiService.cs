using PrayerTimeEngine.Code.Domain.Calculator.Fazilet.Models;

namespace PrayerTimeEngine.Code.Domain.Calculator.Fazilet.Interfaces
{
    public interface IFaziletApiService
    {
        public Task<Dictionary<string, int>> GetCountries();
        public Task<Dictionary<string, int>> GetCitiesByCountryID(int countryID);
        public Task<List<FaziletPrayerTimes>> GetTimesByCityID(int cityID);
    }
}
