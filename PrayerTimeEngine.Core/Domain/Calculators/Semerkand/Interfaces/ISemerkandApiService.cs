using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces
{
    public interface ISemerkandApiService
    {
        public Task<Dictionary<string, int>> GetCountries();
        public Task<Dictionary<string, int>> GetCitiesByCountryID(int countryID);
        public Task<List<SemerkandPrayerTimes>> GetTimesByCityID(DateTime date, int cityID);
    }
}
