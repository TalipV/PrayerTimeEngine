using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;

public interface ISemerkandRepository
{
    Task<List<SemerkandCountry>> GetCountries(CancellationToken cancellationToken);
    Task<List<SemerkandCity>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);
    Task<SemerkandDailyPrayerTimes> GetTimesByDateAndCityID(LocalDate date, int cityID, CancellationToken cancellationToken);

    Task InsertCountries(IEnumerable<SemerkandCountry> countries, CancellationToken cancellationToken);
    Task InsertCities(IEnumerable<SemerkandCity> cities, CancellationToken cancellationToken);
    Task InsertPrayerTimesAsync(IEnumerable<SemerkandDailyPrayerTimes> semerkandPrayerTimesLst, CancellationToken cancellationToken);
    Task<int?> GetCityIDByName(int countryID, string cityName, CancellationToken cancellationToken);
    Task<bool> HasCityData(int countryID, CancellationToken cancellationToken);
    Task<bool> HasCountryData(CancellationToken cancellationToken);
    Task<int?> GetCountryIDByName(string countryName, CancellationToken cancellationToken);
}
