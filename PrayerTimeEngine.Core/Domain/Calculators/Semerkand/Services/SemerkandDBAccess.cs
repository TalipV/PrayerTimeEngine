using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services
{
    public class SemerkandDBAccess(
            AppDbContext dbContext
        ) : ISemerkandDBAccess
    {
        public Task<bool> HasCountryData(CancellationToken cancellationToken)
        {
            return dbContext
                .SemerkandCountries
                .AnyAsync(cancellationToken);
        }

        public Task<int?> GetCountryIDByName(string countryName, CancellationToken cancellationToken)
        {
            return dbContext
                .SemerkandCountries
                .Where(x => x.Name == countryName)
                .Select(x => (int?)x.ID)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<List<SemerkandCountry>> GetCountries(CancellationToken cancellationToken)
        {
            return dbContext
                .SemerkandCountries.AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<List<SemerkandCity>> GetCitiesByCountryID(int countryId, CancellationToken cancellationToken)
        {
            return dbContext
                .SemerkandCities.AsNoTrackingWithIdentityResolution()
                .Where(x => x.CountryID == countryId)
                .Include(x => x.Country).ThenInclude(x => x.Cities)
                .ToListAsync(cancellationToken);
        }

        public Task<bool> HasCityData(int countryID, CancellationToken cancellationToken)
        {
            return dbContext
                .SemerkandCities
                .Where(x => x.CountryID == countryID)
                .AnyAsync(cancellationToken);
        }

        public Task<int?> GetCityIDByName(int countryID, string cityName, CancellationToken cancellationToken)
        {
            return dbContext
                .SemerkandCities
                .Where(x => x.CountryID == countryID && x.Name == cityName)
                .Select(x => (int?)x.ID)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<SemerkandPrayerTimes> GetTimesByDateAndCityID(LocalDate date, int cityId, CancellationToken cancellationToken)
        {
            return dbContext
                .SemerkandPrayerTimes.AsNoTracking()
                .Where(x => x.Date == date && x.CityID == cityId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task InsertCountry(int id, string name, CancellationToken cancellationToken)
        {
            await dbContext.SemerkandCountries.AddAsync(new SemerkandCountry { ID = id, Name = name }, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task InsertCity(int id, string name, int countryId, CancellationToken cancellationToken)
        {
            await dbContext.SemerkandCities.AddAsync(new SemerkandCity { ID = id, Name = name, CountryID = countryId }, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task InsertSemerkandPrayerTimes(LocalDate date, int cityID, SemerkandPrayerTimes semerkandPrayerTimes, CancellationToken cancellationToken)
        {
            await dbContext.SemerkandPrayerTimes.AddAsync(semerkandPrayerTimes, cancellationToken).ConfigureAwait(false);
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

        public async Task DeleteAllPrayerTimes(CancellationToken cancellationToken)
        {
            List<SemerkandPrayerTimes> toBeDeletedTimes = await dbContext.SemerkandPrayerTimes.ToListAsync(cancellationToken).ConfigureAwait(false);
            dbContext.SemerkandPrayerTimes.RemoveRange(toBeDeletedTimes);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
