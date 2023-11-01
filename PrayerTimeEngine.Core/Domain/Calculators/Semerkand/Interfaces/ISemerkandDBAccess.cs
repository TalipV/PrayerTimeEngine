using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces
{
    public interface ISemerkandDBAccess
    {
        public Task<List<SemerkandCountry>> GetCountries();
        public Task<List<SemerkandCity>> GetCitiesByCountryID(int countryID);
        public Task<SemerkandPrayerTimes> GetTimesByDateAndCityID(LocalDate date, int cityID);

        public Task InsertCountries(Dictionary<string, int> countries);
        public Task InsertCities(Dictionary<string, int> cities, int countryId);
        public Task InsertSemerkandPrayerTimes(LocalDate date, int cityID, SemerkandPrayerTimes faziletPrayerTimes);
    }
}
