using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces
{
    public interface IProfileService
    {
        public Task<List<Profile>> GetProfiles();
        public Task SaveProfile(Profile profile);

        public GenericSettingConfiguration GetTimeConfig(Profile profile, ETimeType timeType);
        public BaseLocationData GetLocationConfig(Profile profile, ECalculationSource calculationSource);

        public Task UpdateLocationConfig(Profile profile, string locationName, List<(ECalculationSource CalculationSource, BaseLocationData LocationData)> locationDataByCalculationSource);
        public Task UpdateTimeConfig(Profile profile, ETimeType timeType, GenericSettingConfiguration settings);

        public string GetLocationDataDisplayText(Profile profile);
        public string GetPrayerTimeConfigDisplayText(Profile profile);

        bool EqualsFullProfile(Profile profile1, Profile profile2);
        List<GenericSettingConfiguration> GetActiveComplexTimeConfigs(Profile profile);
        Task<Profile> GetUntrackedReferenceOfProfile(int profileID);
    }
}
