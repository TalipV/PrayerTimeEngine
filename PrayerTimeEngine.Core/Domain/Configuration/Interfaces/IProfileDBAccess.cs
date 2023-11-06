using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Core.Domain.Configuration.Interfaces
{
    public interface IProfileDBAccess
    {
        public Task<List<Profile>> GetProfiles();
        public Task SaveProfile(Profile profile);
        public Task UpdateLocationConfig(Profile profile, string locationName, Dictionary<ECalculationSource, BaseLocationData> locationDataByCalculationSource);
    }
}
