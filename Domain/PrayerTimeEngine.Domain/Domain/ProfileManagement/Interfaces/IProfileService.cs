using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;
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

    /// <summary>
    /// Sets the place of the profile and looks up the location data of every provider for it.
    /// A failing provider does not prevent the others; its failure is part of the result.
    /// </summary>
    Task<List<LocationLookupResult>> UpdateLocationConfig(DynamicProfile profile, ProfilePlaceInfo placeInfo, CancellationToken cancellationToken);

    /// <summary>
    /// Looks up the location data of the providers that have none for the profile's place yet
    /// (e.g. because a service was unavailable when the place was selected) and stores the found ones.
    /// </summary>
    /// <param name="providers">Only these providers are looked up (if their location data is missing).</param>
    /// <returns>The result per looked up provider.</returns>
    Task<List<LocationLookupResult>> CompleteMissingLocationData(DynamicProfile profile, IReadOnlyCollection<EDynamicPrayerTimeProviderType> providers, CancellationToken cancellationToken);

    Task UpdateTimeConfig(DynamicProfile profile, ETimeType timeType, GenericSettingConfiguration settings, CancellationToken cancellationToken);

    string GetLocationDataDisplayText(DynamicProfile profile);
    string GetPrayerTimeConfigDisplayText(DynamicProfile profile);

    List<GenericSettingConfiguration> GetActiveComplexTimeConfigs(DynamicProfile profile);
    /// <summary>
    /// Creates a mosque profile. Without <paramref name="mosqueName"/> the profile is named after the app and the identifier.
    /// </summary>
    Task<MosqueProfile> CreateNewMosqueProfile(EMosquePrayerTimeProviderType selectedItem, string externalID, string? mosqueName, CancellationToken cancellationToken);
    DateTimeZone GetDateTimeZone(Profile profile);
    Task ChangeProfileName(Profile profile, string newProfileName, CancellationToken cancellationToken);
    ZonedDateTime GetCurrentZonedDateTime(DynamicProfile profile);

    /// <summary>
    /// increasing counter per profile, bumped on every mutation.
    /// Allows cache validity to be checked without a DB round-trip.
    /// </summary>
    long GetProfileVersion(int profileID);
}
