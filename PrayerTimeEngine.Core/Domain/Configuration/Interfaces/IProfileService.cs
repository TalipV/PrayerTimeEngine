using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Core.Domain.Configuration.Interfaces
{
    public interface IProfileService
    {
        public Task<List<Profile>> GetProfiles();
        public Task SaveProfile(Profile profile);

        public GenericSettingConfiguration GetTimeConfig(Profile profile, ETimeType timeType, bool createIfNotExists = false);
        public void SetTimeConfig(Profile profile, ETimeType timeType, GenericSettingConfiguration settings);

        public BaseLocationData GetLocationConfig(Profile profile, ECalculationSource calculationSource);
        public Task UpdateLocationConfig(Profile profile, string locationName, List<(ECalculationSource CalculationSource, BaseLocationData LocationData)> locationDataByCalculationSource);
    }
}
