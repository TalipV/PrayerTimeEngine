using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces
{
    public interface IFaziletDBAccess
    {
        public Task<List<FaziletCountry>> GetCountries(CancellationToken cancellationToken);
        public Task<List<FaziletCity>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);
        public Task<FaziletPrayerTimes> GetTimesByDateAndCityID(LocalDate date, int cityID, CancellationToken cancellationToken);

        public Task InsertCountries(Dictionary<string, int> countries, CancellationToken cancellationToken);
        public Task InsertCities(Dictionary<string, int> cities, int countryId, CancellationToken cancellationToken);
        public Task InsertFaziletPrayerTimesIfNotExists(LocalDate date, int cityID, FaziletPrayerTimes faziletPrayerTimes, CancellationToken cancellationToken);
        public Task<bool> HasCountryData(CancellationToken cancellationToken);
        public Task<int?> GetCountryIDByName(string countryName, CancellationToken cancellationToken);
        public Task<bool> HasCityData(int countryID, CancellationToken cancellationToken);
        public Task<int?> GetCityIDByName(int countryID, string cityName, CancellationToken cancellationToken);
    }
}
