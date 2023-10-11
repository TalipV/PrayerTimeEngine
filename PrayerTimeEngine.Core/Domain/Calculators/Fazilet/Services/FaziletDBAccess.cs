using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services
{
    public class FaziletDBAccess(
            AppDbContext dbContext
        ) : IFaziletDBAccess
    {
        public async Task<Dictionary<string, int>> GetCountries()
        {
            List<FaziletCountry> countries = await dbContext.FaziletCountries.ToListAsync().ConfigureAwait(false);
            return countries.ToDictionary(x => x.Name, x => x.ID);
        }

        public async Task InsertCountry(int id, string name)
        {
            await dbContext.FaziletCountries.AddAsync(new FaziletCountry { ID = id, Name = name }).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task InsertCountries(Dictionary<string, int> countries)
        {
            foreach (var item in countries)
            {
                await InsertCountry(item.Value, item.Key).ConfigureAwait(false);
            }
        }

        public async Task InsertCity(int id, string name, int countryId)
        {
            await dbContext.FaziletCities.AddAsync(new FaziletCity { ID = id, Name = name, CountryID = countryId }).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task InsertCities(Dictionary<string, int> cities, int countryId)
        {
            foreach (var item in cities)
            {
                await InsertCity(item.Value, item.Key, countryId).ConfigureAwait(false);
            }
        }

        public async Task<Dictionary<string, int>> GetCitiesByCountryID(int countryId)
        {
            List<FaziletCity> cities = await dbContext.FaziletCities.Where(x => x.CountryID == countryId).ToListAsync().ConfigureAwait(false);
            return cities.ToDictionary(x => x.Name, x => x.ID);
        }

        public async Task<FaziletPrayerTimes> GetTimesByDateAndCityID(LocalDate date, int cityId)
        {
            return await dbContext.FaziletPrayerTimes.Where(x => x.Date == date && x.CityID == cityId).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task InsertFaziletPrayerTimesIfNotExists(LocalDate date, int cityID, FaziletPrayerTimes faziletPrayerTimes)
        {
            await dbContext.FaziletPrayerTimes.AddAsync(faziletPrayerTimes).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAllTimes()
        {
            dbContext.FaziletPrayerTimes.RemoveRange(await dbContext.FaziletPrayerTimes.ToListAsync().ConfigureAwait(false));
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
