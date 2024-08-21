using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces
{
    public interface IProfileService
    {
        public Task<List<Profile>> GetProfiles(CancellationToken cancellationToken);
        public Task SaveProfile(Profile profile, CancellationToken cancellationToken);

        public GenericSettingConfiguration GetTimeConfig(Profile profile, ETimeType timeType);
        public BaseLocationData GetLocationConfig(Profile profile, ECalculationSource calculationSource);

        public Task UpdateLocationConfig(Profile profile, ProfilePlaceInfo placeInfo, List<(ECalculationSource CalculationSource, BaseLocationData LocationData)> locationDataByCalculationSource, CancellationToken cancellationToken);
        public Task UpdateTimeConfig(Profile profile, ETimeType timeType, GenericSettingConfiguration settings, CancellationToken cancellationToken);

        public string GetLocationDataDisplayText(Profile profile);
        public string GetPrayerTimeConfigDisplayText(Profile profile);

        List<GenericSettingConfiguration> GetActiveComplexTimeConfigs(Profile profile);
        Task<Profile> GetUntrackedReferenceOfProfile(int profileID, CancellationToken cancellationToken);
    }
}
