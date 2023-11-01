using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces
{
    public interface IFaziletDBAccess
    {
        public Task<List<FaziletCountry>> GetCountries();
        public Task<List<FaziletCity>> GetCitiesByCountryID(int countryID);
        public Task<FaziletPrayerTimes> GetTimesByDateAndCityID(LocalDate date, int cityID);

        public Task InsertCountries(Dictionary<string, int> countries);
        public Task InsertCities(Dictionary<string, int> cities, int countryId);
        public Task InsertFaziletPrayerTimesIfNotExists(LocalDate date, int cityID, FaziletPrayerTimes faziletPrayerTimes);
    }
}
