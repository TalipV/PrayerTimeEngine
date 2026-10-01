using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;

public interface IProfileRepository
{
    Task<List<Profile>> GetProfiles(CancellationToken cancellationToken);
    Task SaveProfile(Profile profile, CancellationToken cancellationToken);
    Task SaveProfiles(ICollection<Profile> profiles, CancellationToken cancellationToken);

    Task UpdateLocationConfig(DynamicProfile profile, ProfilePlaceInfo placeInfo, List<(EDynamicPrayerTimeProviderType DynamicPrayerTimeProvider, BaseLocationData LocationData)> locationDataByDynamicPrayerTimeProvider, string newProfileName, CancellationToken cancellationToken);
    Task UpdateTimeConfig(DynamicProfile profile, ETimeType timeType, GenericSettingConfiguration settings, CancellationToken cancellationToken);

    Task<Profile> GetUntrackedReferenceOfProfile(int profileID, CancellationToken cancellationToken);
    Task<Profile> CopyProfile(Profile profile, CancellationToken cancellationToken);
    Task DeleteProfile(Profile profile, CancellationToken cancellationToken);
    Task<MosqueProfile> CreateNewMosqueProfile(EMosquePrayerTimeProviderType providerType, string externalID, string profileName, CancellationToken cancellationToken);
    Task ChangeProfileName(Profile profile, string newProfileName, CancellationToken cancellationToken);
}
