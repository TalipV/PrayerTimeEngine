using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Services;

public class ProfileDBAccess(
        IDbContextFactory<AppDbContext> dbContextFactory
    ) : IProfileDBAccess
{
    public async Task<Profile> GetUntrackedReferenceOfProfile(int profileID, CancellationToken cancellationToken)
    {
        using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            return await includeGeneralData(dbContext.Profiles)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == profileID, cancellationToken);
        }
    }

    private static IIncludableQueryable<Profile, TimezoneInfo> includeGeneralData(IQueryable<Profile> queryable)
    {
        return queryable
            .Include(x => ((DynamicProfile)x).TimeConfigs)
            .Include(x => ((DynamicProfile)x).LocationConfigs)
            .Include(x => ((DynamicProfile)x).PlaceInfo).ThenInclude(x => x.TimezoneInfo);
    }

    public async Task<List<Profile>> GetProfiles(CancellationToken cancellationToken)
    {
        using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            return await includeGeneralData(dbContext.Profiles)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }

    public async Task SaveProfile(Profile profile, CancellationToken cancellationToken)
    {
        using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            Profile foundProfile =
                await includeGeneralData(dbContext.Profiles)
                    .FirstOrDefaultAsync(x => x.ID == profile.ID, cancellationToken)
                    .ConfigureAwait(false);

            if (foundProfile != null)
            {
                if (foundProfile is DynamicProfile foundDynamicProfile)
                {
                    dbContext.TimezoneInfos.Remove(foundDynamicProfile.PlaceInfo.TimezoneInfo);
                    dbContext.PlaceInfos.Remove(foundDynamicProfile.PlaceInfo);
                }
                dbContext.Profiles.Remove(foundProfile);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await dbContext.Profiles.AddAsync(profile, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<Profile> CopyProfile(Profile profile, CancellationToken cancellationToken)
    {
        using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            Profile clonedProfile = dbContext.DetachedClone(profile);
            clonedProfile.ID = default;
            clonedProfile.SequenceNo = dbContext.Profiles.Select(x => x.SequenceNo).Max() + 1;
            
            if (clonedProfile is DynamicProfile clonedDynamicProfile 
                && profile is DynamicProfile dynamicProfile)
            {
                clonedDynamicProfile.PlaceInfo = dbContext.DetachedClone(dynamicProfile.PlaceInfo);
                clonedDynamicProfile.PlaceInfo.ID = default;
                clonedDynamicProfile.PlaceInfo.TimezoneInfo = dbContext.DetachedClone(dynamicProfile.PlaceInfo.TimezoneInfo);
                clonedDynamicProfile.PlaceInfo.TimezoneInfo.ID = default;

                await dbContext.Entry(clonedDynamicProfile).Collection(x => x.TimeConfigs).LoadAsync(cancellationToken);
                foreach (var timeConfig in dynamicProfile.TimeConfigs)
                {
                    ProfileTimeConfig copiedTimeConfig = dbContext.DetachedClone(timeConfig);
                    copiedTimeConfig.ID = default;
                    clonedDynamicProfile.TimeConfigs.Add(copiedTimeConfig);
                }

                await dbContext.Entry(clonedDynamicProfile).Collection(x => x.LocationConfigs).LoadAsync(cancellationToken);
                foreach (var locationConfig in dynamicProfile.LocationConfigs)
                {
                    ProfileLocationConfig copiedProfileLocationConfig = dbContext.DetachedClone(locationConfig);
                    copiedProfileLocationConfig.ID = default;
                    clonedDynamicProfile.LocationConfigs.Add(copiedProfileLocationConfig);
                }
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

    public async Task UpdateLocationConfig(
        DynamicProfile inputProfile,
        ProfilePlaceInfo newPlaceInfo,
        List<(EDynamicPrayerTimeProviderType DynamicPrayerTimeProvider, BaseLocationData LocationData)> locationDataByDynamicPrayerTimeProvider,
        CancellationToken cancellationToken)
    {
        using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            DynamicProfile dynamicProfile =
                await dbContext.DynamicProfiles
                    .Include(x => x.LocationConfigs)
                    .Include(x => x.PlaceInfo).ThenInclude(x => x.TimezoneInfo)
                    .FirstOrDefaultAsync(x => x.ID == inputProfile.ID, cancellationToken)
                    .ConfigureAwait(false);
            try
            {
                setNewLocationData(dbContext, dynamicProfile, locationDataByDynamicPrayerTimeProvider);

                if (dynamicProfile.PlaceInfo != null)
                {
                    if (dynamicProfile.PlaceInfo.TimezoneInfo != null)
                    {
                        dbContext.TimezoneInfos.Remove(dynamicProfile.PlaceInfo.TimezoneInfo);
                    }
                    dbContext.PlaceInfos.Remove(dynamicProfile.PlaceInfo);
                }
                dynamicProfile.PlaceInfo = newPlaceInfo;
                dynamicProfile.PlaceInfo.ProfileID = dynamicProfile.ID;
                dynamicProfile.PlaceInfo.Profile = dynamicProfile;

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
        DynamicProfile inputProfile,
        ETimeType timeType,
        GenericSettingConfiguration settings,
        CancellationToken cancellationToken)
    {
        using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            DynamicProfile dynamicTrackedProfil =
                await dbContext.DynamicProfiles
                    .Include(x => x.TimeConfigs)
                    .FirstOrDefaultAsync(x => x.ID == inputProfile.ID, cancellationToken)
                    .ConfigureAwait(false);
            try
            {
                setTimeConfig(dynamicTrackedProfil, timeType, settings);
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
        DynamicProfile profile,
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
        DynamicProfile profile, ETimeType timeType,
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
