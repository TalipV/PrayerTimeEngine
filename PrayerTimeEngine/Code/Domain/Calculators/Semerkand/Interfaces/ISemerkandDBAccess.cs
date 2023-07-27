using PrayerTimeEngine.Code.Domain.Calculator.Semerkand.Models;

namespace PrayerTimeEngine.Code.Domain.Calculators.Semerkand.Interfaces
{
    public interface ISemerkandDBAccess
    {
        public Task<Dictionary<string, int>> GetCountries();
        public Task<Dictionary<string, int>> GetCitiesByCountryID(int countryID);

        public Task<SemerkandPrayerTimes> GetTimesByDateAndCityID(DateTime date, int cityID);

        public Task InsertCountries(Dictionary<string, int> countries);
        public Task InsertCities(Dictionary<string, int> cities, int countryId);
        public Task InsertSemerkandPrayerTimes(DateTime date, int cityID, SemerkandPrayerTimes faziletPrayerTimes);
    }
}
