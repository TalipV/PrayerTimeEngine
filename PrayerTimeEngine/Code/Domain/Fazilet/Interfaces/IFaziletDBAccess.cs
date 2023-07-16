using PrayerTimeEngine.Code.Domain.Fazilet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.Fazilet.Interfaces
{
    public interface IFaziletDBAccess
    {
        public Dictionary<string, int> GetCountries();
        public Dictionary<string, int> GetCitiesByCountryID(int countryID);

        public FaziletPrayerTimes GetTimesByDateAndCityID(DateTime date, int cityID);

        public void InsertCountries(Dictionary<string, int> countries);
        public void InsertCities(Dictionary<string, int> cities, int countryId);
        public void InsertFaziletPrayerTimes(DateTime date, int cityID, FaziletPrayerTimes faziletPrayerTimes);
    }
}
