using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Models;

namespace PrayerTimeEngine.Core.Domain.Configuration.Services
{
    public class ConfigStoreService(
            IConfigStoreDBAccess configStoreDBAccess
        ) : IConfigStoreService
    {
        public async Task<List<Profile>> GetProfiles()
        {
            return await configStoreDBAccess.GetProfiles().ConfigureAwait(false);
        }

        public async Task SaveProfile(Profile profile)
        {
            await configStoreDBAccess.SaveProfile(profile).ConfigureAwait(false);
        }
    }
}
