using PrayerTimeEngine.Domain.Calculators.Fazilet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Domain.Calculators.Fazilet.Interfaces
{
    public interface IFaziletDBAccess
    {
        public Task<Dictionary<string, int>> GetCountries();
        public Task<Dictionary<string, int>> GetCitiesByCountryID(int countryID);

        public Task<FaziletPrayerTimes> GetTimesByDateAndCityID(DateTime date, int cityID);

        public Task InsertCountries(Dictionary<string, int> countries);
        public Task InsertCities(Dictionary<string, int> cities, int countryId);
        public Task InsertFaziletPrayerTimes(DateTime date, int cityID, FaziletPrayerTimes faziletPrayerTimes);
    }
}
