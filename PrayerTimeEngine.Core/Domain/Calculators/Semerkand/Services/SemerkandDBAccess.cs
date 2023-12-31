﻿using Microsoft.EntityFrameworkCore;
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
        public async Task<List<SemerkandCountry>> GetCountries()
        {
            return await dbContext
                .SemerkandCountries.AsNoTracking()
                .ToListAsync().ConfigureAwait(false);
        }
        public async Task<List<SemerkandCity>> GetCitiesByCountryID(int countryId)
        {
            return await dbContext
                .SemerkandCities.AsNoTracking()
                .Where(x => x.CountryID == countryId)
                .ToListAsync().ConfigureAwait(false);
        }
        public async Task<SemerkandPrayerTimes> GetTimesByDateAndCityID(LocalDate date, int cityId)
        {
            return await dbContext
                .SemerkandPrayerTimes.AsNoTracking()
                .Where(x => x.Date == date && x.CityID == cityId)
                .FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task InsertCountry(int id, string name)
        {
            await dbContext.SemerkandCountries.AddAsync(new SemerkandCountry { ID = id, Name = name }).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task InsertCity(int id, string name, int countryId)
        {
            await dbContext.SemerkandCities.AddAsync(new SemerkandCity { ID = id, Name = name, CountryID = countryId }).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task InsertSemerkandPrayerTimes(LocalDate date, int cityID, SemerkandPrayerTimes semerkandPrayerTimes)
        {
            await dbContext.SemerkandPrayerTimes.AddAsync(semerkandPrayerTimes).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task InsertCountries(Dictionary<string, int> countries)
        {
            foreach (var item in countries)
            {
                await InsertCountry(item.Value, item.Key).ConfigureAwait(false);
            }
        }

        public async Task InsertCities(Dictionary<string, int> cities, int countryId)
        {
            foreach (var item in cities)
            {
                await InsertCity(item.Value, item.Key, countryId).ConfigureAwait(false);
            }
        }

        public async Task DeleteAllPrayerTimes()
        {
            dbContext.SemerkandPrayerTimes.RemoveRange(await dbContext.SemerkandPrayerTimes.ToListAsync().ConfigureAwait(false));
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
