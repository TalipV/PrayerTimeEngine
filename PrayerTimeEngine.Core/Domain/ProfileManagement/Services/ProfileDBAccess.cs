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
        public Task<Profile> GetUntrackedReferenceOfProfile(int profileID, CancellationToken cancellationToken)
        {
            return dbContext.Profiles
                .Include(x => x.TimeConfigs)
                .Include(x => x.LocationConfigs)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == profileID, cancellationToken);
        }

        public Task<List<Profile>> GetProfiles(CancellationToken cancellationToken)
        {
            return dbContext.Profiles
                .Include(x => x.TimeConfigs)
                .Include(x => x.LocationConfigs)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task SaveProfile(Profile profile, CancellationToken cancellationToken)
        {
            if (await dbContext.Profiles.FindAsync(keyValues: [profile.ID], cancellationToken).ConfigureAwait(false) is Profile foundProfile)
            {
                dbContext.Profiles.Remove(foundProfile);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await dbContext.Profiles.AddAsync(profile, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateLocationConfig(
            Profile profile,
            string locationName,
            List<(ECalculationSource CalculationSource, BaseLocationData LocationData)> locationDataByCalculationSource,
            CancellationToken cancellationToken)
        {
            Profile trackedProfile = await dbContext.Profiles.FindAsync(keyValues: [profile.ID], cancellationToken).ConfigureAwait(false);

            try
            {
                using (IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false))
                {
                    await setNewLocationData(trackedProfile, locationDataByCalculationSource, cancellationToken).ConfigureAwait(false);
                    trackedProfile.LocationName = locationName;

                    await SaveProfile(trackedProfile, cancellationToken).ConfigureAwait(false);
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                dbContext.Entry(trackedProfile).State = EntityState.Detached;

                await dbContext.Entry(profile).ReloadAsync(CancellationToken.None).ConfigureAwait(false);

                foreach (var locationConfig in profile.LocationConfigs)
                    await dbContext.Entry(locationConfig).ReloadAsync(CancellationToken.None).ConfigureAwait(false);
            }
        }

        public async Task UpdateTimeConfig(Profile profile, ETimeType timeType, GenericSettingConfiguration settings, CancellationToken cancellationToken)
        {
            Profile trackedProfile = await dbContext.Profiles.FindAsync(keyValues: [profile.ID], cancellationToken).ConfigureAwait(false);

            try
            {
                using (IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false))
                {
                    setTimeConfig(profile, timeType, settings);
                    await SaveProfile(profile, cancellationToken).ConfigureAwait(false);
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                dbContext.Entry(trackedProfile).State = EntityState.Detached;

                await dbContext.Entry(profile).ReloadAsync(CancellationToken.None).ConfigureAwait(false);

                foreach (var timeConfig in profile.TimeConfigs)
                    await dbContext.Entry(timeConfig).ReloadAsync(CancellationToken.None).ConfigureAwait(false);
            }
        }

        private async Task setNewLocationData(
            Profile profile, 
            List<(ECalculationSource CalculationSource, BaseLocationData LocationData)> locationDataByCalculationSource,
            CancellationToken cancellationToken)
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
            await dbContext.ProfileLocations.AddRangeAsync(profile.LocationConfigs, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private static void setTimeConfig(
            Profile profile, ETimeType timeType, 
            GenericSettingConfiguration settings)
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
