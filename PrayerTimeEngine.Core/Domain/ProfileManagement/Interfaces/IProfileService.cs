using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;

public interface IProfileService
{
    public Task<List<Profile>> GetProfiles(CancellationToken cancellationToken);
    Task<Profile> GetUntrackedReferenceOfProfile(int profileID, CancellationToken cancellationToken);
    
    public Task SaveProfile(Profile profile, CancellationToken cancellationToken);
    Task DeleteProfile(Profile profile, CancellationToken cancellationToken);
    Task<Profile> CopyProfile(Profile profile, CancellationToken cancellationToken);

    public GenericSettingConfiguration GetTimeConfig(DynamicProfile profile, ETimeType timeType);
    public BaseLocationData GetLocationConfig(DynamicProfile profile, EDynamicPrayerTimeProviderType dynamicPrayerTimeProviderType);

    public Task UpdateLocationConfig(DynamicProfile profile, ProfilePlaceInfo placeInfo, List<(EDynamicPrayerTimeProviderType DynamicPrayerTimeProvider, BaseLocationData LocationData)> locationDataByDynamicPrayerTimeProvider, CancellationToken cancellationToken);
    public Task UpdateTimeConfig(DynamicProfile profile, ETimeType timeType, GenericSettingConfiguration settings, CancellationToken cancellationToken);

    public string GetLocationDataDisplayText(DynamicProfile profile);
    public string GetPrayerTimeConfigDisplayText(DynamicProfile profile);

    List<GenericSettingConfiguration> GetActiveComplexTimeConfigs(DynamicProfile profile);
    Task<MosqueProfile> CreateNewMosqueProfile(EMosquePrayerTimeProviderType selectedItem, string externalID, CancellationToken cancellationToken);
    DateTimeZone GetDateTimeZone(Profile profile);
    Task ChangeProfileName(Profile profile, string newProfileName, CancellationToken cancellationToken);
}
