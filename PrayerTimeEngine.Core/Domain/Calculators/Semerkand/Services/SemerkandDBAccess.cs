using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.DTOs;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services
{
    public class SemerkandDBAccess(
            AppDbContext dbContext
        ) : ISemerkandDBAccess
    {
        public Task<List<SemerkandCountry>> GetCountries(CancellationToken cancellationToken)
        {
            return dbContext
                .SemerkandCountries.AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<bool> HasCountryData(CancellationToken cancellationToken)
        {
            return dbContext
                .SemerkandCountries
                .AnyAsync(cancellationToken);
        }

        private static readonly Func<AppDbContext, string, Task<int?>> compiledQuery_GetCountryIDByName =
            EF.CompileAsyncQuery(
                (AppDbContext context, string countryName) =>
                    context.SemerkandCountries
                        .Where(x => x.Name == countryName)
                        .Select(x => (int?)x.ID)
                        .FirstOrDefault());
        public Task<int?> GetCountryIDByName(string countryName, CancellationToken cancellationToken)
        {
            // cancellation?
            return compiledQuery_GetCountryIDByName(dbContext, countryName);
        }

        private static readonly Func<AppDbContext, int, IAsyncEnumerable<SemerkandCity>> compiledQuery_GetCitiesByCountryID =
            EF.CompileAsyncQuery(
                (AppDbContext context, int countryId) =>
                    context.SemerkandCities
                        .AsNoTrackingWithIdentityResolution()
                        .Include(x => x.Country)
                        .ThenInclude(x => x.Cities)
                        .Where(x => x.CountryID == countryId));
        public Task<List<SemerkandCity>> GetCitiesByCountryID(int countryId, CancellationToken cancellationToken)
        {
            return compiledQuery_GetCitiesByCountryID(dbContext, countryId)
                .ToListAsync(cancellationToken)
                .AsTask();  // I don't know ValueTasks
        }

        public Task<bool> HasCityData(int countryID, CancellationToken cancellationToken)
        {
            return dbContext
                .SemerkandCities
                .Where(x => x.CountryID == countryID)
                .AnyAsync(cancellationToken);
        }

        private static readonly Func<AppDbContext, string, Task<int?>> compiledQuery_GetCityIDByName =
            EF.CompileAsyncQuery(
                (AppDbContext context, string cityName) =>
                    context.SemerkandCities
                        .Where(x => x.Name == cityName)
                        .Select(x => (int?)x.ID)
                        .FirstOrDefault());
        public Task<int?> GetCityIDByName(int countryID, string cityName, CancellationToken cancellationToken)
        {
            // cancellation?
            return compiledQuery_GetCityIDByName(dbContext, cityName);
        }

        private static readonly Func<AppDbContext, LocalDate, int, Task<SemerkandPrayerTimes>> compiledQuery_GetTimesByDateAndCityID =
            EF.CompileAsyncQuery(
                (AppDbContext context, LocalDate date, int cityId) =>
                    context.SemerkandPrayerTimes
                        .AsNoTracking()
                        .Where(x => x.Date == date && x.CityID == cityId)
                        .FirstOrDefault());
        public Task<SemerkandPrayerTimes> GetTimesByDateAndCityID(LocalDate date, int cityId, CancellationToken cancellationToken)
        {
            // cancellation?
            return compiledQuery_GetTimesByDateAndCityID(dbContext, date, cityId);
        }

        public async Task InsertSemerkandPrayerTimes(LocalDate date, int cityID, SemerkandPrayerTimes semerkandPrayerTimes, CancellationToken cancellationToken)
        {
            await dbContext.SemerkandPrayerTimes.AddAsync(semerkandPrayerTimes, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task InsertCountries(IEnumerable<SemerkandCountry> countries, CancellationToken cancellationToken)
        {
            await dbContext.SemerkandCountries.AddRangeAsync(countries, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task InsertCities(IEnumerable<SemerkandCity> cities, CancellationToken cancellationToken)
        {
            await dbContext.SemerkandCities.AddRangeAsync(cities, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAllPrayerTimes(CancellationToken cancellationToken)
        {
            List<SemerkandPrayerTimes> toBeDeletedTimes = await dbContext.SemerkandPrayerTimes.ToListAsync(cancellationToken).ConfigureAwait(false);
            dbContext.SemerkandPrayerTimes.RemoveRange(toBeDeletedTimes);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
