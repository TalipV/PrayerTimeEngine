using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces
{
    public interface IProfileDBAccess
    {
        public Task<List<Profile>> GetProfiles();
        public Task<Profile> GetUntrackedReferenceOfProfile(int profileID);
        public Task SaveProfile(Profile profile);

        public Task UpdateLocationConfig(Profile profile, string locationName, List<(ECalculationSource CalculationSource, BaseLocationData LocationData)> locationDataByCalculationSource);
        public Task UpdateTimeConfig(Profile profile, ETimeType timeType, GenericSettingConfiguration settings);
    }
}
