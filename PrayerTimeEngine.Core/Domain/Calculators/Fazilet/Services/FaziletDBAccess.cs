using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services
{
    public class FaziletDBAccess(
            AppDbContext dbContext
        ) : IFaziletDBAccess
    {
        public Task<List<FaziletCountry>> GetCountries(CancellationToken cancellationToken)
        {
            return dbContext
                .FaziletCountries.AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<bool> HasCountryData(CancellationToken cancellationToken)
        {
            return dbContext
                .FaziletCountries
                .AnyAsync(cancellationToken);
        }

        public Task<int?> GetCountryIDByName(string countryName, CancellationToken cancellationToken)
        {
            return dbContext
                .FaziletCountries
                .Where(x => x.Name == countryName)
                .Select(x => (int?)x.ID)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static readonly Func<AppDbContext, int, IAsyncEnumerable<FaziletCity>> compiledQuery_GetCitiesByCountryID =
            EF.CompileAsyncQuery(
                (AppDbContext context, int countryId) =>
                    context.FaziletCities.AsNoTrackingWithIdentityResolution()
                .Include(x => x.Country).ThenInclude(x => x.Cities).Where(x => x.CountryID == countryId));

        public async Task<List<FaziletCity>> GetCitiesByCountryID(int countryId, CancellationToken cancellationToken)
        {
            var returnLst = new List<FaziletCity>();

            await foreach (var s in compiledQuery_GetCitiesByCountryID(dbContext, countryId).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                returnLst.Add(s);
            }

            return returnLst;
        }

        public Task<bool> HasCityData(int countryID, CancellationToken cancellationToken)
        {
            return dbContext
                .FaziletCities
                .Where(x => x.CountryID == countryID)
                .AnyAsync(cancellationToken);
        }

        public Task<int?> GetCityIDByName(int countryID, string cityName, CancellationToken cancellationToken)
        {
            return dbContext
                .FaziletCities
                .Where(x => x.CountryID == countryID && x.Name == cityName)
                .Select(x => (int?)x.ID)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<FaziletPrayerTimes> GetTimesByDateAndCityID(LocalDate date, int cityId, CancellationToken cancellationToken)
        {
            return dbContext
                .FaziletPrayerTimes.AsNoTracking()
                .Where(x => x.Date == date && x.CityID == cityId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task InsertCountry(int id, string name, CancellationToken cancellationToken)
        {
            await dbContext.FaziletCountries.AddAsync(new FaziletCountry { ID = id, Name = name }, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task InsertCity(int id, string name, int countryId, CancellationToken cancellationToken)
        {
            await dbContext.FaziletCities.AddAsync(new FaziletCity { ID = id, Name = name, CountryID = countryId }, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task InsertFaziletPrayerTimesIfNotExists(LocalDate date, int cityID, FaziletPrayerTimes faziletPrayerTimes, CancellationToken cancellationToken)
        {
            await dbContext.FaziletPrayerTimes.AddAsync(faziletPrayerTimes, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task InsertCountries(Dictionary<string, int> countries, CancellationToken cancellationToken)
        {
            foreach (var item in countries)
            {
                await InsertCountry(item.Value, item.Key, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task InsertCities(Dictionary<string, int> cities, int countryId, CancellationToken cancellationToken)
        {
            foreach (var item in cities)
            {
                await InsertCity(item.Value, item.Key, countryId, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task DeleteAllTimes(CancellationToken cancellationToken)
        {
            List<FaziletPrayerTimes> toBeRemovedTimes = await dbContext.FaziletPrayerTimes.ToListAsync(cancellationToken).ConfigureAwait(false);
            dbContext.FaziletPrayerTimes.RemoveRange(toBeRemovedTimes);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
