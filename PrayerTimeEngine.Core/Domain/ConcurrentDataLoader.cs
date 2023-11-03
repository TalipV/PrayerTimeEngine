using Microsoft.EntityFrameworkCore;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Models;

namespace PrayerTimeEngine.Core.Domain
{
    public class ConcurrentDataLoader(IProfileService profileService, AppDbContext dbContext)
    {
        public Task<List<Profile>> LoadAllProfilesTask { get; private set; }

        public void InitiateConcurrentDataLoad()
        {
            LoadAllProfilesTask = Task.Run(async () => 
            {
                await dbContext.Database.MigrateAsync();
                return await profileService.GetProfiles();
            });
        }
    }
}
