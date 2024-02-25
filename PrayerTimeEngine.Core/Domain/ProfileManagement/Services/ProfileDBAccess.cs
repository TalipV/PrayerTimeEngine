using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Services
{
    public class ProfileDBAccess(
            AppDbContext dbContext
        ) : IProfileDBAccess
    {
        public async Task<Profile> GetUntrackedReferenceOfProfile(int profileID)
        {
            return await dbContext.Profiles
                .Include(x => x.TimeConfigs)
                .Include(x => x.LocationConfigs)
                .AsNoTracking()
                .FirstAsync(x => x.ID == profileID).ConfigureAwait(false);
        }

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
            List<(ECalculationSource CalculationSource, BaseLocationData LocationData)> locationDataByCalculationSource)
        {
            Profile trackedProfile = dbContext.Profiles.Find(profile.ID);

            try
            {
                using (IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    await setNewLocationData(trackedProfile, locationDataByCalculationSource);
                    trackedProfile.LocationName = locationName;

                    await SaveProfile(trackedProfile);
                    await transaction.CommitAsync();
                }
            }
            finally
            {
                dbContext.Entry(trackedProfile).State = EntityState.Detached;

                await dbContext.Entry(profile).ReloadAsync();

                foreach (var locationConfig in profile.LocationConfigs)
                    await dbContext.Entry(locationConfig).ReloadAsync();
            }
        }

        public async Task UpdateTimeConfig(Profile profile, ETimeType timeType, GenericSettingConfiguration settings)
        {
            Profile trackedProfile = dbContext.Profiles.Find(profile.ID);

            try
            {
                using (IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    setTimeConfig(profile, timeType, settings);
                    await SaveProfile(profile);
                    await transaction.CommitAsync();
                }
            }
            finally
            {
                dbContext.Entry(trackedProfile).State = EntityState.Detached;

                await dbContext.Entry(profile).ReloadAsync();

                foreach (var timeConfig in profile.TimeConfigs)
                    await dbContext.Entry(timeConfig).ReloadAsync();
            }
        }

        private async Task setNewLocationData(Profile profile, List<(ECalculationSource CalculationSource, BaseLocationData LocationData)> locationDataByCalculationSource)
        {
            // delete the old entries
            var currentLocationConfigs = profile.LocationConfigs.ToList();
            dbContext.ProfileLocations.RemoveRange(currentLocationConfigs);
            profile.LocationConfigs.Clear();

            foreach ((ECalculationSource calculationSource, BaseLocationData locationData) in locationDataByCalculationSource)
            {
                var newLocationConfig =
                    new ProfileLocationConfig
                    {
                        CalculationSource = calculationSource,
                        ProfileID = profile.ID,
                        Profile = profile,
                        LocationData = locationData
                    };

                profile.LocationConfigs.Add(newLocationConfig);
            }
            await dbContext.ProfileLocations.AddRangeAsync(profile.LocationConfigs);
            await dbContext.SaveChangesAsync();
        }

        private void setTimeConfig(Profile profile, ETimeType timeType, GenericSettingConfiguration settings)
        {
            if (profile.TimeConfigs.FirstOrDefault(x => x.TimeType == timeType) is not ProfileTimeConfig timeConfig)
            {
                timeConfig = new ProfileTimeConfig();
                profile.TimeConfigs.Add(timeConfig);
            }

            timeConfig.TimeType = timeType;
            timeConfig.ProfileID = profile.ID;
            timeConfig.Profile = profile;
            timeConfig.CalculationConfiguration = settings;
        }
    }
}
