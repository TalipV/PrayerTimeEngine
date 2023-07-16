using PrayerTimeEngine.Code.Domain.Fazilet.Models;

namespace PrayerTimeEngine.Code.Domain.Fazilet.Interfaces
{
    public interface IFaziletApiService
    {
        public Dictionary<string, int> GetCountries();
        public Dictionary<string, int> GetCitiesByCountryID(int countryID);
        public List<FaziletPrayerTimes> GetTimesByCityID(int cityID);
    }
}
