using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces
{
    public interface ISemerkandDBAccess
    {
        public Task<List<SemerkandCountry>> GetCountries(CancellationToken cancellationToken);
        public Task<List<SemerkandCity>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);
        public Task<SemerkandPrayerTimes> GetTimesByDateAndCityID(ZonedDateTime date, int cityID, CancellationToken cancellationToken);

        public Task InsertCountries(IEnumerable<SemerkandCountry> countries, CancellationToken cancellationToken);
        public Task InsertCities(IEnumerable<SemerkandCity> cities, CancellationToken cancellationToken);
        public Task InsertSemerkandPrayerTimes(IEnumerable<SemerkandPrayerTimes> semerkandPrayerTimesLst, CancellationToken cancellationToken);
        public Task<int?> GetCityIDByName(int countryID, string cityName, CancellationToken cancellationToken);
        public Task<bool> HasCityData(int countryID, CancellationToken cancellationToken);
        public Task<bool> HasCountryData(CancellationToken cancellationToken);
        public Task<int?> GetCountryIDByName(string countryName, CancellationToken cancellationToken);
    }
}
