using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Models;

namespace PrayerTimeEngine.Core.Domain.Configuration.Services
{
    public class ConfigStoreService : IConfigStoreService
    {
        IConfigStoreDBAccess _configStoreDBAccess;

        public ConfigStoreService(IConfigStoreDBAccess configStoreDBAccess)
        {
            _configStoreDBAccess = configStoreDBAccess;
        }

        public async Task<List<Profile>> GetProfiles()
        {
            return await _configStoreDBAccess.GetProfiles();
        }

        public async Task SaveProfile(Profile profile)
        {
            await _configStoreDBAccess.SaveProfile(profile);
        }
    }
}
