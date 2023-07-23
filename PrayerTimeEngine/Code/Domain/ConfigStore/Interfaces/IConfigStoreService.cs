using PrayerTimeEngine.Code.Domain.ConfigStore.Models;

namespace PrayerTimeEngine.Code.Domain.ConfigStore.Interfaces
{
    public interface IConfigStoreService
    {
        public Task<List<Profile>> GetProfiles();
        public Task SaveProfiles(List<Profile> profiles);
    }
}
