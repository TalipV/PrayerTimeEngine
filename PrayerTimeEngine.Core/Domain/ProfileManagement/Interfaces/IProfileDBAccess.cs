using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;

public interface IProfileDBAccess
{
    public Task<List<Profile>> GetProfiles(CancellationToken cancellationToken);
    public Task SaveProfile(Profile profile, CancellationToken cancellationToken);
    public Task SaveProfiles(ICollection<Profile> profiles, CancellationToken cancellationToken);

    public Task UpdateLocationConfig(DynamicProfile profile, ProfilePlaceInfo placeInfo, List<(EDynamicPrayerTimeProviderType DynamicPrayerTimeProvider, BaseLocationData LocationData)> locationDataByDynamicPrayerTimeProvider, CancellationToken cancellationToken);
    public Task UpdateTimeConfig(DynamicProfile profile, ETimeType timeType, GenericSettingConfiguration settings, CancellationToken cancellationToken);

    public Task<Profile> GetUntrackedReferenceOfProfile(int profileID, CancellationToken cancellationToken);
    public Task<Profile> CopyProfile(Profile profile, CancellationToken cancellationToken);
    public Task DeleteProfile(Profile profile, CancellationToken cancellationToken);
    Task<MosqueProfile> CreateNewMosqueProfile(EMosquePrayerTimeProviderType providerType, string externalID, CancellationToken cancellationToken);
    Task ChangeProfileName(Profile profile, string newProfileName, CancellationToken cancellationToken);
}
