using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services
{
    public class SemerkandDBAccess(
            IDbContextFactory<AppDbContext> dbContextFactory
        ) : ISemerkandDBAccess, IPrayerTimeCacheCleaner
    {
        public async Task<List<SemerkandCountry>> GetCountries(CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await dbContext
                    .SemerkandCountries.AsNoTracking()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<bool> HasCountryData(CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await dbContext
                    .SemerkandCountries
                    .AnyAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private static readonly Func<AppDbContext, string, Task<int?>> compiledQuery_GetCountryIDByName =
            EF.CompileAsyncQuery(
                (AppDbContext context, string countryName) =>
                    context.SemerkandCountries
                        .Where(x => x.Name == countryName)
                        .Select(x => (int?)x.ID)
                        .FirstOrDefault());

        public async Task<int?> GetCountryIDByName(string countryName, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await compiledQuery_GetCountryIDByName(dbContext, countryName).ConfigureAwait(false);
            }
        }

        private static readonly Func<AppDbContext, int, IAsyncEnumerable<SemerkandCity>> compiledQuery_GetCitiesByCountryID =
            EF.CompileAsyncQuery(
                (AppDbContext context, int countryId) =>
                    context.SemerkandCities
                        .AsNoTrackingWithIdentityResolution()
                        .Include(x => x.Country).ThenInclude(x => x.Cities)
                        .Where(x => x.CountryID == countryId));

        public async Task<List<SemerkandCity>> GetCitiesByCountryID(int countryId, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await compiledQuery_GetCitiesByCountryID(dbContext, countryId)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<bool> HasCityData(int countryID, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await dbContext
                    .SemerkandCities
                    .Where(x => x.CountryID == countryID)
                    .AnyAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private static readonly Func<AppDbContext, string, Task<int?>> compiledQuery_GetCityIDByName =
            EF.CompileAsyncQuery(
                (AppDbContext context, string cityName) =>
                    context.SemerkandCities
                        .Where(x => x.Name == cityName)
                        .Select(x => (int?)x.ID)
                        .FirstOrDefault());

        public async Task<int?> GetCityIDByName(int countryID, string cityName, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await compiledQuery_GetCityIDByName(dbContext, cityName).ConfigureAwait(false);
            }
        }

        private static readonly Func<AppDbContext, ZonedDateTime, int, Task<SemerkandPrayerTimes>> compiledQuery_GetTimesByDateAndCityID =
            EF.CompileAsyncQuery(
                (AppDbContext context, ZonedDateTime date, int cityId) =>
                    context.SemerkandPrayerTimes
                        .AsNoTracking()
                        .Where(x => x.Date == date && x.CityID == cityId)
                        .FirstOrDefault());

        public async Task<SemerkandPrayerTimes> GetTimesByDateAndCityID(ZonedDateTime date, int cityId, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await compiledQuery_GetTimesByDateAndCityID(dbContext, date, cityId).ConfigureAwait(false);
            }
        }

        public async Task InsertPrayerTimesAsync(IEnumerable<SemerkandPrayerTimes> semerkandPrayerTimesLst, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                await dbContext.SemerkandPrayerTimes.AddRangeAsync(semerkandPrayerTimesLst, cancellationToken).ConfigureAwait(false);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task InsertCountries(IEnumerable<SemerkandCountry> countries, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                await dbContext.SemerkandCountries.AddRangeAsync(countries, cancellationToken: cancellationToken).ConfigureAwait(false);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task InsertCities(IEnumerable<SemerkandCity> cities, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                await dbContext.SemerkandCities.AddRangeAsync(cities, cancellationToken: cancellationToken).ConfigureAwait(false);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task DeleteCacheDataAsync(ZonedDateTime deleteBeforeDate, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                await dbContext.SemerkandPrayerTimes
                    .Where(p => p.Date.ToInstant() < deleteBeforeDate.ToInstant())
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
