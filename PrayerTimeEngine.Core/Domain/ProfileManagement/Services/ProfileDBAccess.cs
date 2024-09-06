using Microsoft.EntityFrameworkCore;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Services
{
    public class ProfileDBAccess(
            IDbContextFactory<AppDbContext> dbContextFactory
        ) : IProfileDBAccess
    {
        public async Task<Profile> GetUntrackedReferenceOfProfile(int profileID, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await dbContext.Profiles
                    .Include(x => x.TimeConfigs)
                    .Include(x => x.LocationConfigs)
                    .Include(x => x.PlaceInfo).ThenInclude(x => x.TimezoneInfo)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ID == profileID, cancellationToken);
            }
        }

        public async Task<List<Profile>> GetProfiles(CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await dbContext.Profiles
                    .Include(x => x.TimeConfigs)
                    .Include(x => x.LocationConfigs)
                    .Include(x => x.PlaceInfo).ThenInclude(x => x.TimezoneInfo)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task SaveProfile(Profile profile, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                Profile foundProfile =
                    await dbContext.Profiles
                        .Include(x => x.TimeConfigs)
                        .Include(x => x.LocationConfigs)
                        .Include(x => x.PlaceInfo).ThenInclude(x => x.TimezoneInfo)
                        .FirstOrDefaultAsync(x => x.ID == profile.ID, cancellationToken)
                        .ConfigureAwait(false);

                if (foundProfile != null)
                {
                    dbContext.TimezoneInfos.Remove(foundProfile.PlaceInfo.TimezoneInfo);
                    dbContext.PlaceInfos.Remove(foundProfile.PlaceInfo);
                    dbContext.Profiles.Remove(foundProfile);
                    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                await dbContext.Profiles.AddAsync(profile, cancellationToken).ConfigureAwait(false);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task UpdateLocationConfig(
            Profile inputProfile,
            ProfilePlaceInfo newPlaceInfo,
            List<(EDynamicPrayerTimeProviderType DynamicPrayerTimeProvider, BaseLocationData LocationData)> locationDataByDynamicPrayerTimeProvider,
            CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                Profile profile =
                    await dbContext.Profiles
                        .Include(x => x.LocationConfigs)
                        .Include(x => x.PlaceInfo).ThenInclude(x => x.TimezoneInfo)
                        .FirstOrDefaultAsync(x => x.ID == inputProfile.ID, cancellationToken)
                        .ConfigureAwait(false);
                try
                {
                    setNewLocationData(dbContext, profile, locationDataByDynamicPrayerTimeProvider);

                    if (profile.PlaceInfo != null)
                    {
                        if (profile.PlaceInfo.TimezoneInfo != null)
                        {
                            dbContext.TimezoneInfos.Remove(profile.PlaceInfo.TimezoneInfo);
                        }
                        dbContext.PlaceInfos.Remove(profile.PlaceInfo);
                    }
                    profile.PlaceInfo = newPlaceInfo;
                    profile.PlaceInfo.ProfileID = profile.ID;
                    profile.PlaceInfo.Profile = profile;

                    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    dbContext.ChangeTracker.Clear();

                    await dbContext.Entry(inputProfile).ReloadAsync(cancellationToken).ConfigureAwait(false);
                    await dbContext.Entry(inputProfile).Reference(x => x.PlaceInfo).LoadAsync(cancellationToken);
                    await dbContext.Entry(inputProfile.PlaceInfo).Reference(x => x.TimezoneInfo).LoadAsync(cancellationToken).ConfigureAwait(false);
                    await dbContext.Entry(inputProfile).Collection(x => x.LocationConfigs).ReloadAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task UpdateTimeConfig(
            Profile inputProfile, 
            ETimeType timeType, 
            GenericSettingConfiguration settings, 
            CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                Profile trackedProfile =
                    await dbContext.Profiles
                        .Include(x => x.TimeConfigs)
                        .FirstOrDefaultAsync(x => x.ID == inputProfile.ID, cancellationToken)
                        .ConfigureAwait(false);
                try
                {
                    setTimeConfig(trackedProfile, timeType, settings);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                finally
                {
                    // or a new db context for this reloading stuff?
                    dbContext.ChangeTracker.Clear();

                    await dbContext.Entry(inputProfile).Collection(x => x.TimeConfigs).ReloadAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static void setNewLocationData(
            AppDbContext dbContext,
            Profile profile,
            List<(EDynamicPrayerTimeProviderType DynamicPrayerTimeProvider, BaseLocationData LocationData)> locationDataByDynamicPrayerTimeProvider)
        {
            var currentLocationConfigs = profile.LocationConfigs.ToList();

            HashSet<EDynamicPrayerTimeProviderType> newDynamicPrayerTimeProviders = locationDataByDynamicPrayerTimeProvider.Select(x => x.DynamicPrayerTimeProvider).ToHashSet();
            List<ProfileLocationConfig> configsToRemove = currentLocationConfigs
                .Where(config => !newDynamicPrayerTimeProviders.Contains(config.DynamicPrayerTimeProvider))
                .ToList();

            foreach ((EDynamicPrayerTimeProviderType dynamicPrayerTimeProviderType, BaseLocationData locationData) in locationDataByDynamicPrayerTimeProvider)
            {
                var existingLocationConfig = currentLocationConfigs
                    .FirstOrDefault(config => config.DynamicPrayerTimeProvider == dynamicPrayerTimeProviderType);

                if (existingLocationConfig != null)
                {
                    existingLocationConfig.LocationData = locationData;
                }
                else
                {
                    var newLocationConfig = new ProfileLocationConfig
                    {
                        DynamicPrayerTimeProvider = dynamicPrayerTimeProviderType,
                        ProfileID = profile.ID,
                        Profile = profile,
                        LocationData = locationData
                    };

                    profile.LocationConfigs.Add(newLocationConfig);
                }
            }

            dbContext.ProfileLocations.RemoveRange(configsToRemove);
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

        public async Task<Profile> CopyProfile(Profile profile, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                Profile clonedProfile = dbContext.DetachedClone(profile);
                clonedProfile.ID = default;
                clonedProfile.SequenceNo = dbContext.Profiles.Select(x => x.SequenceNo).Max() + 1;
                
                clonedProfile.PlaceInfo = dbContext.DetachedClone(profile.PlaceInfo);
                clonedProfile.PlaceInfo.ID = default;
                clonedProfile.PlaceInfo.TimezoneInfo = dbContext.DetachedClone(profile.PlaceInfo.TimezoneInfo);
                clonedProfile.PlaceInfo.TimezoneInfo.ID = default;

                await dbContext.Entry(clonedProfile).Collection(x => x.TimeConfigs).LoadAsync(cancellationToken);
                foreach (var timeConfig in profile.TimeConfigs)
                {
                    ProfileTimeConfig copiedTimeConfig = dbContext.DetachedClone(timeConfig);
                    copiedTimeConfig.ID = default;
                    clonedProfile.TimeConfigs.Add(copiedTimeConfig);
                }

                await dbContext.Entry(clonedProfile).Collection(x => x.LocationConfigs).LoadAsync(cancellationToken);
                foreach (var locationConfig in profile.LocationConfigs)
                {
                    ProfileLocationConfig copiedProfileLocationConfig = dbContext.DetachedClone(locationConfig);
                    copiedProfileLocationConfig.ID = default;
                    clonedProfile.LocationConfigs.Add(copiedProfileLocationConfig);
                }

                await dbContext.Profiles.AddAsync(clonedProfile, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                return clonedProfile;
            }
        }

        public async Task DeleteProfile(Profile profile, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                Profile trackedProfile = await this.GetUntrackedReferenceOfProfile(profile.ID, cancellationToken);
                dbContext.Entry(trackedProfile).State = EntityState.Unchanged;

                dbContext.Profiles.Remove(trackedProfile);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
