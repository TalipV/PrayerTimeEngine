using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;

public interface IProfileService
{
    Task<List<Profile>> GetProfiles(CancellationToken cancellationToken);
    Task<Profile> GetUntrackedReferenceOfProfile(int profileID, CancellationToken cancellationToken);

    Task SaveProfile(Profile profile, CancellationToken cancellationToken);
    Task DeleteProfile(Profile profile, CancellationToken cancellationToken);
    Task<Profile> CopyProfile(Profile profile, CancellationToken cancellationToken);

    GenericSettingConfiguration GetTimeConfig(DynamicProfile profile, ETimeType timeType);
    BaseLocationData GetLocationConfig(DynamicProfile profile, EDynamicPrayerTimeProviderType dynamicPrayerTimeProviderType);

    Task UpdateLocationConfig(DynamicProfile profile, ProfilePlaceInfo placeInfo, CancellationToken cancellationToken);
    Task UpdateTimeConfig(DynamicProfile profile, ETimeType timeType, GenericSettingConfiguration settings, CancellationToken cancellationToken);

    string GetLocationDataDisplayText(DynamicProfile profile);
    string GetPrayerTimeConfigDisplayText(DynamicProfile profile);

    List<GenericSettingConfiguration> GetActiveComplexTimeConfigs(DynamicProfile profile);
    Task<MosqueProfile> CreateNewMosqueProfile(EMosquePrayerTimeProviderType selectedItem, string externalID, CancellationToken cancellationToken);
    DateTimeZone GetDateTimeZone(Profile profile);
    Task ChangeProfileName(Profile profile, string newProfileName, CancellationToken cancellationToken);
    ZonedDateTime GetCurrentZonedDateTime(DynamicProfile profile);

    /// <summary>
    /// increasing counter per profile, bumped on every mutation.
    /// Allows cache validity to be checked without a DB round-trip.
    /// </summary>
    long GetProfileVersion(int profileID);
}
