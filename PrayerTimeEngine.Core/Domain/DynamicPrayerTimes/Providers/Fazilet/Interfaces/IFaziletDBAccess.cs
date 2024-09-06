using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces
{
    public interface IFaziletDBAccess
    {
        public Task<List<FaziletCountry>> GetCountries(CancellationToken cancellationToken);
        public Task<List<FaziletCity>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);
        public Task<FaziletDailyPrayerTimes> GetTimesByDateAndCityID(ZonedDateTime date, int cityID, CancellationToken cancellationToken);

        public Task InsertCountries(IEnumerable<FaziletCountry> countries, CancellationToken cancellationToken);
        public Task InsertCities(IEnumerable<FaziletCity> cities, CancellationToken cancellationToken);
        public Task InsertPrayerTimesAsync(IEnumerable<FaziletDailyPrayerTimes> faziletPrayerTimesLst, CancellationToken cancellationToken);
        public Task<bool> HasCountryData(CancellationToken cancellationToken);
        public Task<int?> GetCountryIDByName(string countryName, CancellationToken cancellationToken);
        public Task<bool> HasCityData(int countryID, CancellationToken cancellationToken);
        public Task<int?> GetCityIDByName(int countryID, string cityName, CancellationToken cancellationToken);
    }
}
