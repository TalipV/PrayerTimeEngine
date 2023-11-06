using MethodTimer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Core.Domain.Configuration.Services
{
    public class ProfileDBAccess(
            AppDbContext dbContext
        ) : IProfileDBAccess
    {

        [Time]
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

        public async Task UpdateLocationConfig(
            Profile profile,
            string locationName,
            Dictionary<ECalculationSource, BaseLocationData> locationDataByCalculationSource)
        {
            Profile trackedProfile = dbContext.Profiles.Find(profile.ID);

            try
            {
                using (IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    await this.SetNewLocationData(trackedProfile, locationDataByCalculationSource);
                    trackedProfile.LocationName = locationName;

                    await this.SaveProfile(trackedProfile);
                    await transaction.CommitAsync();
                }
            }
            finally
            {
                dbContext.Entry(trackedProfile).State = EntityState.Detached;

                await dbContext.Entry(profile).ReloadAsync();
                foreach (var locationConfig in profile.LocationConfigs)
                    await dbContext.Entry(locationConfig).ReloadAsync();
                foreach (var timeConfig in profile.TimeConfigs)
                    await dbContext.Entry(timeConfig).ReloadAsync();
            }
        }

        public async Task SetNewLocationData(Profile profile, Dictionary<ECalculationSource, BaseLocationData> locationDataByCalculationSource)
        {
            // delete the old entries
            var currentLocationConfigs = profile.LocationConfigs.ToList();
            dbContext.ProfileLocations.RemoveRange(currentLocationConfigs);
            profile.LocationConfigs.Clear();

            foreach (KeyValuePair<ECalculationSource, BaseLocationData> locationData in locationDataByCalculationSource)
            {
                var newLocationConfig =
                    new ProfileLocationConfig
                    {
                        CalculationSource = locationData.Key,
                        ProfileID = profile.ID,
                        Profile = profile,
                        LocationData = locationData.Value
                    };

                profile.LocationConfigs.Add(newLocationConfig);
            }
            await dbContext.ProfileLocations.AddRangeAsync(profile.LocationConfigs);
            await dbContext.SaveChangesAsync();
        }
    }
}
