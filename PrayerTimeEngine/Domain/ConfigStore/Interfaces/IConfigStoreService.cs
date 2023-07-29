using PrayerTimeEngine.Domain.ConfigStore.Models;

namespace PrayerTimeEngine.Domain.ConfigStore.Interfaces
{
    public interface IConfigStoreService
    {
        public Task<List<Profile>> GetProfiles();
        public Task SaveProfiles(List<Profile> profiles);
    }
}
