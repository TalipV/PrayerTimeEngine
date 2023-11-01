using Microsoft.EntityFrameworkCore;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Models;

namespace PrayerTimeEngine.Core.Domain.Configuration.Services
{
    public class ProfileDBAccess(
            AppDbContext dbContext
        ) : IProfileDBAccess
    {
        public async Task<List<Profile>> GetProfiles()
        {
            return await dbContext.Profiles
                .Include(x => x.TimeConfigs)
                .Include(x => x.LocationConfigs)
                .AsNoTracking()
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task SaveProfile(Profile profile)
        {
            if (await dbContext.Profiles.FindAsync(profile.ID) is Profile foundProfile)
            {
                dbContext.Profiles.Remove(foundProfile);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }

            await dbContext.Profiles.AddAsync(profile).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
