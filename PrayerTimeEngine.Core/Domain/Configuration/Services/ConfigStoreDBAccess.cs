using Microsoft.EntityFrameworkCore;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Models;

namespace PrayerTimeEngine.Core.Domain.Configuration.Services
{
    public class ConfigStoreDBAccess(
            AppDbContext dbContext
        ) : IConfigStoreDBAccess
    {
        public async Task<List<Profile>> GetProfiles()
        {
            return await dbContext.Profiles
                .Include(x => x.TimeConfigs)
                .Include(x => x.LocationConfigs)
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task SaveProfile(Profile profile)
        {
            await DeleteProfile(profile);
            await dbContext.Profiles.AddAsync(profile).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteProfile(Profile profile)
        {
            dbContext.Profiles.Remove(profile);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
