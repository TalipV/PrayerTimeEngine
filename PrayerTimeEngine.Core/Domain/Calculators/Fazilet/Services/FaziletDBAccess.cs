using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services
{
    public class FaziletDBAccess(
            AppDbContext dbContext
        ) : IFaziletDBAccess, IPrayerTimeCacheCleaner
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

        private static readonly Func<AppDbContext, string, Task<int?>> compiledQuery_GetCountryIDByName =
            EF.CompileAsyncQuery(
                (AppDbContext context, string countryName) =>
                    context.FaziletCountries
                        .Where(x => x.Name == countryName)
                        .Select(x => (int?)x.ID)
                        .FirstOrDefault());
        public Task<int?> GetCountryIDByName(string countryName, CancellationToken cancellationToken)
        {
            // cancellation?
            return compiledQuery_GetCountryIDByName(dbContext, countryName);
        }

        private static readonly Func<AppDbContext, int, IAsyncEnumerable<FaziletCity>> compiledQuery_GetCitiesByCountryID =
            EF.CompileAsyncQuery(
                (AppDbContext context, int countryId) =>
                    context.FaziletCities
                        .AsNoTrackingWithIdentityResolution()
                        .Include(x => x.Country).ThenInclude(x => x.Cities) // why?
                        .Where(x => x.CountryID == countryId));
        public Task<List<FaziletCity>> GetCitiesByCountryID(int countryId, CancellationToken cancellationToken)
        {
            return compiledQuery_GetCitiesByCountryID(dbContext, countryId)
                .ToListAsync(cancellationToken)
                .AsTask();  // I don't know ValueTasks
        }

        public Task<bool> HasCityData(int countryID, CancellationToken cancellationToken)
        {
            return dbContext
                .FaziletCities
                .Where(x => x.CountryID == countryID)
                .AnyAsync(cancellationToken);
        }

        private static readonly Func<AppDbContext, string, Task<int?>> compiledQuery_GetCityIDByName =
            EF.CompileAsyncQuery(
                (AppDbContext context, string cityName) =>
                    context.FaziletCities
                        .Where(x => x.Name == cityName)
                        .Select(x => (int?)x.ID)
                        .FirstOrDefault());
        public Task<int?> GetCityIDByName(int countryID, string cityName, CancellationToken cancellationToken)
        {
            // cancellation?
            return compiledQuery_GetCityIDByName(dbContext, cityName);
        }

        private static readonly Func<AppDbContext, ZonedDateTime, int, Task<FaziletPrayerTimes>> compiledQuery_GetTimesByDateAndCityID =
            EF.CompileAsyncQuery(
                (AppDbContext context, ZonedDateTime date, int cityId) =>
                    context.FaziletPrayerTimes
                        .AsNoTracking()
                        .Where(x => x.Date == date && x.CityID == cityId)
                        .FirstOrDefault());
        public Task<FaziletPrayerTimes> GetTimesByDateAndCityID(ZonedDateTime date, int cityId, CancellationToken cancellationToken)
        {
            // cancellation?
            return compiledQuery_GetTimesByDateAndCityID(dbContext, date, cityId);
        }

        public async Task InsertFaziletPrayerTimes(IEnumerable<FaziletPrayerTimes> faziletPrayerTimesLst, CancellationToken cancellationToken)
        {
            await dbContext.FaziletPrayerTimes.AddRangeAsync(faziletPrayerTimesLst, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task InsertCountries(IEnumerable<FaziletCountry> countries, CancellationToken cancellationToken)
        {
            await dbContext.FaziletCountries.AddRangeAsync(countries, cancellationToken: cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task InsertCities(IEnumerable<FaziletCity> cities, CancellationToken cancellationToken)
        {
            await dbContext.FaziletCities.AddRangeAsync(cities, cancellationToken: cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteCacheDataAsync(ZonedDateTime deleteBeforeDate, CancellationToken cancellationToken)
        {
            await dbContext.FaziletPrayerTimes
                .Where(p => p.Date.ToInstant() < deleteBeforeDate.ToInstant())
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
