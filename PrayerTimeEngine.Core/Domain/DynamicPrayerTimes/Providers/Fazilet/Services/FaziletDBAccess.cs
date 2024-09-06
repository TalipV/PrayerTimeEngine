using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services
{
    public class FaziletDBAccess(
            IDbContextFactory<AppDbContext> dbContextFactory
        ) : IFaziletDBAccess, IPrayerTimeCacheCleaner
    {
        public async Task<List<FaziletCountry>> GetCountries(CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await dbContext
                    .FaziletCountries.AsNoTracking()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<bool> HasCountryData(CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await dbContext
                    .FaziletCountries
                    .AnyAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private static readonly Func<AppDbContext, string, Task<int?>> compiledQuery_GetCountryIDByName =
            EF.CompileAsyncQuery(
                (AppDbContext context, string countryName) =>
                    context.FaziletCountries
                        .Where(x => x.Name == countryName)
                        .Select(x => (int?)x.ID)
                        .FirstOrDefault());

        public async Task<int?> GetCountryIDByName(string countryName, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                // cancellation?
                return await compiledQuery_GetCountryIDByName(dbContext, countryName).ConfigureAwait(false);
            }
        }

        private static readonly Func<AppDbContext, int, IAsyncEnumerable<FaziletCity>> compiledQuery_GetCitiesByCountryID =
            EF.CompileAsyncQuery(
                (AppDbContext context, int countryId) =>
                    context.FaziletCities
                        .AsNoTrackingWithIdentityResolution()
                        .Include(x => x.Country).ThenInclude(x => x.Cities)
                        .Where(x => x.CountryID == countryId));

        public async Task<List<FaziletCity>> GetCitiesByCountryID(int countryId, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await compiledQuery_GetCitiesByCountryID(dbContext, countryId)
                    .ToListAsync(cancellationToken)
                    .AsTask()
                    .ConfigureAwait(false);
            }
        }

        public async Task<bool> HasCityData(int countryID, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await dbContext
                    .FaziletCities
                    .Where(x => x.CountryID == countryID)
                    .AnyAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private static readonly Func<AppDbContext, string, Task<int?>> compiledQuery_GetCityIDByName =
            EF.CompileAsyncQuery(
                (AppDbContext context, string cityName) =>
                    context.FaziletCities
                        .Where(x => x.Name == cityName)
                        .Select(x => (int?)x.ID)
                        .FirstOrDefault());

        public async Task<int?> GetCityIDByName(int countryID, string cityName, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                // cancellation?
                return await compiledQuery_GetCityIDByName(dbContext, cityName).ConfigureAwait(false);
            }
        }

        private static readonly Func<AppDbContext, ZonedDateTime, int, Task<FaziletDailyPrayerTimes>> compiledQuery_GetTimesByDateAndCityID =
            EF.CompileAsyncQuery(
                (AppDbContext context, ZonedDateTime date, int cityId) =>
                    context.FaziletPrayerTimes
                        .AsNoTracking()
                        .Where(x => x.Date == date && x.CityID == cityId)
                        .FirstOrDefault());

        public async Task<FaziletDailyPrayerTimes> GetTimesByDateAndCityID(ZonedDateTime date, int cityId, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                // cancellation?
                return await compiledQuery_GetTimesByDateAndCityID(dbContext, date, cityId).ConfigureAwait(false);
            }
        }

        public async Task InsertPrayerTimesAsync(IEnumerable<FaziletDailyPrayerTimes> faziletPrayerTimesLst, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                await dbContext.FaziletPrayerTimes.AddRangeAsync(faziletPrayerTimesLst, cancellationToken).ConfigureAwait(false);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task InsertCountries(IEnumerable<FaziletCountry> countries, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                await dbContext.FaziletCountries.AddRangeAsync(countries, cancellationToken: cancellationToken).ConfigureAwait(false);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task InsertCities(IEnumerable<FaziletCity> cities, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                await dbContext.FaziletCities.AddRangeAsync(cities, cancellationToken: cancellationToken).ConfigureAwait(false);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task DeleteCacheDataAsync(ZonedDateTime deleteBeforeDate, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                await dbContext.FaziletPrayerTimes
                    .Where(p => p.Date.ToInstant() < deleteBeforeDate.ToInstant())
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
