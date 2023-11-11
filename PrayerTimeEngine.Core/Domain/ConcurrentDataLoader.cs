using Microsoft.EntityFrameworkCore;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Data.Preferences;
using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Core.Domain
{
    public class ConcurrentDataLoader(
            IProfileService profileService,
            AppDbContext dbContext,
            PreferenceService preferenceService
        )
    {
        public Task<(Profile, PrayerTimesBundle)> LoadAllProfilesFromJsonTask { get; private set; }
        public Task<List<Profile>> LoadAllProfilesFromDbTask { get; private set; }

        public void InitiateConcurrentDataLoad()
        {
            LoadAllProfilesFromJsonTask = Task.Run(() =>
            {
                if (preferenceService.GetCurrentProfile() is Profile profile
                    && preferenceService.GetCurrentData(profile) is PrayerTimesBundle prayerTimes)
                {
                    return (profile, prayerTimes);
                }

                return (null, null);
            });

            LoadAllProfilesFromDbTask = Task.Run(async () => 
            {
                await dbContext.Database.MigrateAsync().ConfigureAwait(false);
                return await profileService.GetProfiles().ConfigureAwait(false);
            });
        }
    }
}
