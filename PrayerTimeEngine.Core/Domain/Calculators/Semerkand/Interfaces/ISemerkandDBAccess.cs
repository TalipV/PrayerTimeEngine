using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces
{
    public interface ISemerkandDBAccess
    {
        public Task<List<SemerkandCountry>> GetCountries(CancellationToken cancellationToken);
        public Task<List<SemerkandCity>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);
        public Task<SemerkandPrayerTimes> GetTimesByDateAndCityID(LocalDate date, int cityID, CancellationToken cancellationToken);

        public Task InsertCountries(Dictionary<string, int> countries, CancellationToken cancellationToken);
        public Task InsertCities(Dictionary<string, int> cities, int countryId, CancellationToken cancellationToken);
        public Task InsertSemerkandPrayerTimes(LocalDate date, int cityID, SemerkandPrayerTimes semerkandPrayerTimes, CancellationToken cancellationToken);
        public Task<int?> GetCityIDByName(int countryID, string cityName, CancellationToken cancellationToken);
        public Task<bool> HasCityData(int countryID, CancellationToken cancellationToken);
        public Task<bool> HasCountryData(CancellationToken cancellationToken);
        public Task<int?> GetCountryIDByName(string countryName, CancellationToken cancellationToken);
    }
}
