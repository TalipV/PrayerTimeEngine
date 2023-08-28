using PrayerTimeEngine.Core.Domain.Configuration.Models;

namespace PrayerTimeEngine.Core.Domain.Configuration.Interfaces
{
    public interface IConfigStoreDBAccess
    {
        public Task<List<Profile>> GetProfiles();
        public Task<List<TimeSpecificConfig>> GetTimeSpecificConfigsByProfile(int profileID);
        public Task SaveProfile(Profile profile);
    }
}
