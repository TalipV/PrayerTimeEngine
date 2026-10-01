using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces;

public interface IFaziletRepository
{
    Task<List<FaziletCountry>> GetCountries(CancellationToken cancellationToken);
    Task<List<FaziletCity>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken);
    Task<FaziletDailyPrayerTimes> GetTimesByDateAndCityID(LocalDate date, int cityID, CancellationToken cancellationToken);

    Task InsertCountries(IEnumerable<FaziletCountry> countries, CancellationToken cancellationToken);
    Task InsertCities(IEnumerable<FaziletCity> cities, CancellationToken cancellationToken);
    Task InsertPrayerTimesAsync(IEnumerable<FaziletDailyPrayerTimes> faziletPrayerTimesLst, CancellationToken cancellationToken);
    Task<bool> HasCountryData(CancellationToken cancellationToken);
    Task<int?> GetCountryIDByName(string countryName, CancellationToken cancellationToken);
    Task<bool> HasCityData(int countryID, CancellationToken cancellationToken);
    Task<int?> GetCityIDByName(int countryID, string cityName, CancellationToken cancellationToken);
}
