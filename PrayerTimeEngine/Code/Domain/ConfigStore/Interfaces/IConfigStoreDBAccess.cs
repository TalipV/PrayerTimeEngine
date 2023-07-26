using PrayerTimeEngine.Code.Domain.ConfigStore.Models;

namespace PrayerTimeEngine.Code.Domain.ConfigStore.Interfaces
{
    public interface IConfigStoreDBAccess
    {
        public Task<List<Profile>> GetProfiles();
        public Task<List<TimeSpecificConfig>> GetTimeSpecificConfigsByProfile(int profileID);
        public Task<List<ILocationConfig>> GetLocationConfigsByProfile(int profileID);

        public Task SaveProfile(Profile profile);
    }
}
