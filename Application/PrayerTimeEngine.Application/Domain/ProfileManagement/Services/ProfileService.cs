using System.Text;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Services;

public class ProfileService(
        IProfileRepository profileRepository,
        IDynamicPrayerTimeProviderFactory dynamicPrayerTimeProviderFactory,
        TimeTypeAttributeService timeTypeAttributeService,
        IProfileVersionStore profileVersionStore,
        ILocationLookupStatusStore locationLookupStatusStore,
        ISystemInfoService systemInfoService,
        ILogger<ProfileService> logger
    ) : IProfileService
{
    public long GetProfileVersion(int profileID) => profileVersionStore.GetVersion(profileID);

    private void BumpProfileVersion(int profileID) => profileVersionStore.BumpVersion(profileID);

    public async Task<List<Profile>> GetProfiles(CancellationToken cancellationToken)
    {
        List<Profile> profiles = await profileRepository.GetProfiles(cancellationToken).ConfigureAwait(false);

        if (profiles.Count == 0)
        {
            profiles.Add(GetDefaultProfile());
            await SaveProfile(profiles[0], cancellationToken).ConfigureAwait(false);
        }

        return profiles;
    }

    public async Task SaveProfile(Profile profile, CancellationToken cancellationToken)
    {
        await profileRepository.SaveProfile(profile, cancellationToken).ConfigureAwait(false);
        if (profile.ID > 0)
        {
            BumpProfileVersion(profile.ID);
        }
    }

    private static DynamicProfile GetDefaultProfile()
    {
        var profile = new DynamicProfile
        {
            ID = 1,
            Name = "Standard-Profil",
            PlaceInfo = new ProfilePlaceInfo
            {
                ExternalID = "343647974",
                Latitude = 47.2692124M,
                Longitude = 11.4041024M,
                InfoLanguageCode = "de",
                Country = "Österreich",
                City = "Innsbruck",
                CityDistrict = "",
                PostCode = "6020",
                Street = "",
                TimezoneInfo = new TimezoneInfo
                {
                    DisplayName = "CEST",
                    Name = "Europe/Vienna",
                    UtcOffsetSeconds = 7200
                }
            },
            SequenceNo = 1,
        };

        profile.LocationConfigs =
            [
                new()
                {
                    DynamicPrayerTimeProvider = EDynamicPrayerTimeProviderType.Muwaqqit,
                    ProfileID = profile.ID,
                    Profile = profile,
                    LocationData =
                        new MuwaqqitLocationData
                        {
                            Latitude = 47.2692124M,
                            Longitude = 11.4041024M,
                            TimezoneName = "Europe/Vienna"
                        }
                },
                new()
                {
                    DynamicPrayerTimeProvider = EDynamicPrayerTimeProviderType.Fazilet,
                    ProfileID = profile.ID,
                    Profile = profile,
                    LocationData =
                        new FaziletLocationData
                        {
                            CountryName = "Avusturya",
                            CityName = "Innsbruck"
                        }
                },
                new()
                {
                    DynamicPrayerTimeProvider = EDynamicPrayerTimeProviderType.Semerkand,
                    ProfileID = profile.ID,
                    Profile = profile,
                    LocationData =
                        new SemerkandLocationData
                        {
                            CountryName = "Avusturya",
                            CityName = "Innsbruck",
                            TimezoneName = "Europe/Vienna"
                        }
                },
                new()
                {
                    DynamicPrayerTimeProvider = EDynamicPrayerTimeProviderType.Waqti,
                    ProfileID = profile.ID,
                    Profile = profile,
                    LocationData =
                        new WaqtiLocationData
                        {
                            Latitude = 47.2692124M,
                            Longitude = 11.4041024M,
                            TimezoneName = "Europe/Vienna"
                        }
                },
            ];

        profile.TimeConfigs =
            [
                new()
                {
                    TimeType = ETimeType.FajrStart,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // späteste Berechnung
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Fazilet, TimeType = ETimeType.FajrStart }
                },
                new()
                {
                    TimeType = ETimeType.FajrEnd,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // früheste Berechnung
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Semerkand, MinuteAdjustment = -5, TimeType = ETimeType.FajrEnd }
                },
                new()
                {
                    TimeType = ETimeType.FajrGhalas,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // Grobe Einschätzung anhand von Sichtung  
                    CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Degree = -8.5 }
                },
                new()
                {
                    TimeType = ETimeType.FajrKaraha,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // ### keine Erfahrung
                    CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Degree = -4.0 }
                },
                new()
                {
                    TimeType = ETimeType.DuhaStart,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // von Gebetszeiten-Hoca, späteste
                    CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.DuhaStart, Degree = 5.0 }
                },
                new()
                {
                    TimeType = ETimeType.DuhaEnd,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // ### keine Erfahrung
                    CalculationConfiguration = new GenericSettingConfiguration { TimeType = ETimeType.DuhaEnd, MinuteAdjustment = -25 }
                },
                new()
                {
                    TimeType = ETimeType.DhuhrStart,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // späteste Berechnung
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Fazilet, TimeType = ETimeType.DhuhrStart }
                },
                new()
                {
                    TimeType = ETimeType.DhuhrEnd,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // früheste Berechnung
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Muwaqqit, MinuteAdjustment = -5, TimeType = ETimeType.DhuhrEnd }
                },
                new()
                {
                    TimeType = ETimeType.AsrStart,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // späteste Berechnung
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Fazilet, TimeType = ETimeType.AsrStart }
                },
                new()
                {
                    TimeType = ETimeType.AsrEnd,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // früheste Berechnung
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Muwaqqit, MinuteAdjustment = -5, TimeType = ETimeType.AsrEnd }
                },
                new()
                {
                    TimeType = ETimeType.AsrMithlayn,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Muwaqqit, TimeType = ETimeType.AsrMithlayn }
                },
                new()
                {
                    TimeType = ETimeType.AsrKaraha,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // von Gebetszeiten-Hoca, früheste
                    CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.AsrKaraha, Degree = 5.0 }
                },
                new()
                {
                    TimeType = ETimeType.MaghribStart,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // späteste Berechnung
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Fazilet, TimeType = ETimeType.MaghribStart }
                },
                new()
                {
                    TimeType = ETimeType.MaghribEnd,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // ### keine Erfahrung, aber Sicherheitsabstand
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Semerkand, MinuteAdjustment = -10, TimeType = ETimeType.MaghribEnd }
                },
                new()
                {
                    TimeType = ETimeType.MaghribSufficientTime,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { TimeType = ETimeType.MaghribSufficientTime, MinuteAdjustment = 20 }
                },
                new()
                {
                    TimeType = ETimeType.MaghribIshtibak,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // von Gebetszeiten-Hoca
                    CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribIshtibak, Degree = -10.0 }
                },
                new()
                {
                    TimeType = ETimeType.IshaStart,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // späteste Berechnung
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Fazilet, TimeType = ETimeType.IshaStart }
                },
                new()
                {
                    TimeType = ETimeType.IshaEnd,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // früheste Berechnung
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Semerkand, TimeType = ETimeType.IshaEnd }
                },
                new()
                {
                    TimeType = ETimeType.QiblaTime,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Muwaqqit, TimeType = ETimeType.QiblaTime }
                }
            ];

        return profile;
    }

    public Task<Profile> GetUntrackedReferenceOfProfile(int profileID, CancellationToken cancellationToken)
    {
        return profileRepository.GetUntrackedReferenceOfProfile(profileID, cancellationToken);
    }

    public Task<Profile> CopyProfile(Profile profile, CancellationToken cancellationToken)
    {
        return profileRepository.CopyProfile(profile, cancellationToken);
    }

    public async Task DeleteProfile(Profile profile, CancellationToken cancellationToken)
    {
        // Es muss immer mindestens ein DynamicProfile geben; das letzte darf nicht gelöscht werden.
        // MosqueProfiles unterliegen dieser Einschränkung nicht.
        if (profile is DynamicProfile)
        {
            List<Profile> existingProfiles = await profileRepository.GetProfiles(cancellationToken).ConfigureAwait(false);
            bool anyOtherDynamicProfileExists = existingProfiles.OfType<DynamicProfile>().Any(x => x.ID != profile.ID);

            if (!anyOtherDynamicProfileExists)
            {
                throw new InvalidOperationException("Das letzte dynamische Profil kann nicht gelöscht werden. Es muss immer mindestens ein dynamisches Profil vorhanden sein.");
            }
        }

        await profileRepository.DeleteProfile(profile, cancellationToken).ConfigureAwait(false);
        BumpProfileVersion(profile.ID);
    }

    public GenericSettingConfiguration GetTimeConfig(DynamicProfile profile, ETimeType timeType)
    {
        return profile.TimeConfigs.FirstOrDefault(x => x.TimeType == timeType)?.CalculationConfiguration;
    }

    public BaseLocationData GetLocationConfig(DynamicProfile profile, EDynamicPrayerTimeProviderType dynamicPrayerTimeProviderType)
    {
        return profile.LocationConfigs.FirstOrDefault(x => x.DynamicPrayerTimeProvider == dynamicPrayerTimeProviderType)?.LocationData;
    }

    public async Task<List<LocationLookupResult>> UpdateLocationConfig(
        DynamicProfile profile,
        ProfilePlaceInfo placeInfo,
        CancellationToken cancellationToken)
    {
        // the look up history of the previous place does not apply to the new one
        locationLookupStatusStore.Clear(profile.ID);

        List<LocationLookupResult> results = [];
        foreach (EDynamicPrayerTimeProviderType dynamicPrayerTimeProvider in GetDynamicPrayerTimeProviders())
        {
            results.Add(await LookUpLocationData(profile.ID, dynamicPrayerTimeProvider, placeInfo, cancellationToken).ConfigureAwait(false));
        }

        string newProfileName = $"{placeInfo?.City ?? "-"}, {profile.SequenceNo}";

        await profileRepository.UpdateLocationConfig(
            profile,
            placeInfo,
            [.. results.Where(x => x.IsFound).Select(x => (x.Provider, x.LocationData!))],
            newProfileName,
            cancellationToken).ConfigureAwait(false);
        BumpProfileVersion(profile.ID);

        return results;
    }

    public async Task<List<LocationLookupResult>> CompleteMissingLocationData(
        DynamicProfile profile,
        IReadOnlyCollection<EDynamicPrayerTimeProviderType> providers,
        CancellationToken cancellationToken)
    {
        if (profile?.PlaceInfo is not ProfilePlaceInfo placeInfo)
        {
            return [];
        }

        var providersWithLocationData = profile.LocationConfigs
            .Where(x => x.LocationData != null)
            .Select(x => x.DynamicPrayerTimeProvider)
            .ToHashSet();

        List<LocationLookupResult> results = [];
        foreach (EDynamicPrayerTimeProviderType dynamicPrayerTimeProvider in GetDynamicPrayerTimeProviders().Where(x => providers.Contains(x) && !providersWithLocationData.Contains(x)))
        {
            results.Add(await LookUpLocationData(profile.ID, dynamicPrayerTimeProvider, placeInfo, cancellationToken).ConfigureAwait(false));
        }

        List<(EDynamicPrayerTimeProviderType, BaseLocationData)> foundLocationData =
            [.. results.Where(x => x.IsFound).Select(x => (x.Provider, x.LocationData!))];

        if (foundLocationData.Count != 0)
        {
            await profileRepository.AddLocationData(profile, foundLocationData, cancellationToken).ConfigureAwait(false);
            BumpProfileVersion(profile.ID);
            logger.LogInformation(
                "Missing location data for '{Place}' completed: {@DynamicPrayerTimeProviders}",
                placeInfo.DisplayText,
                foundLocationData.Select(x => x.Item1.ToString()).ToList());
        }

        return results;
    }

    private static IEnumerable<EDynamicPrayerTimeProviderType> GetDynamicPrayerTimeProviders()
    {
        return Enum.GetValues<EDynamicPrayerTimeProviderType>().Where(x => x != EDynamicPrayerTimeProviderType.None);
    }

    /// <summary>
    /// The location data of one provider. A failure of one provider must not prevent the others,
    /// so it is returned (and logged) instead of thrown; a cancellation is still thrown.
    /// Every outcome is recorded, so missing location data can be explained later.
    /// </summary>
    private async Task<LocationLookupResult> LookUpLocationData(
        int profileID,
        EDynamicPrayerTimeProviderType dynamicPrayerTimeProvider,
        ProfilePlaceInfo placeInfo,
        CancellationToken cancellationToken)
    {
        LocationLookupResult result;
        try
        {
            BaseLocationData locationData = await dynamicPrayerTimeProviderFactory
                .GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(dynamicPrayerTimeProvider)
                .GetLocationInfo(placeInfo, cancellationToken)
                .ConfigureAwait(false);

            result = new LocationLookupResult(dynamicPrayerTimeProvider, locationData, Error: null);

            if (result.IsPlaceUnknown)
            {
                logger.LogWarning(
                    "Location data of {DynamicPrayerTimeProvider} for '{Place}' not found: the provider does not know the place",
                    dynamicPrayerTimeProvider, placeInfo.DisplayText);
            }
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(exception,
                "Location data of {DynamicPrayerTimeProvider} for '{Place}' could not be determined: {Reason}",
                dynamicPrayerTimeProvider, placeInfo.DisplayText, exception.Message);

            result = new LocationLookupResult(dynamicPrayerTimeProvider, LocationData: null, exception);
        }

        locationLookupStatusStore.Record(profileID, dynamicPrayerTimeProvider, result.IsPlaceUnknown, result.Error, systemInfoService.GetCurrentInstant());
        return result;
    }

    public async Task UpdateTimeConfig(DynamicProfile profile, ETimeType timeType, GenericSettingConfiguration settings, CancellationToken cancellationToken)
    {
        await profileRepository.UpdateTimeConfig(profile, timeType, settings, cancellationToken).ConfigureAwait(false);
        BumpProfileVersion(profile.ID);
    }

    public string GetLocationDataDisplayText(DynamicProfile profile)
    {
        if (profile is null)
        {
            return string.Empty;
        }

        var muwaqqitLocationData = GetLocationConfig(profile, EDynamicPrayerTimeProviderType.Muwaqqit) as MuwaqqitLocationData;
        var faziletLocationData = GetLocationConfig(profile, EDynamicPrayerTimeProviderType.Fazilet) as FaziletLocationData;
        var semerkandLocationData = GetLocationConfig(profile, EDynamicPrayerTimeProviderType.Semerkand) as SemerkandLocationData;
        var waqtiLocationData = GetLocationConfig(profile, EDynamicPrayerTimeProviderType.Waqti) as WaqtiLocationData;

        return $"""
                Muwaqqit:
                    - Coordinates:  
                    ({muwaqqitLocationData?.Latitude} / {muwaqqitLocationData?.Longitude})
                    - Timezone:     
                    '{muwaqqitLocationData?.TimezoneName}'
                
                Fazilet:
                    - Country 
                    '{faziletLocationData?.CountryName}'
                    - City 
                    '{faziletLocationData?.CityName}'
                
                Semerkand:
                    - Country 
                    '{semerkandLocationData?.CountryName}'
                    - City 
                    '{semerkandLocationData?.CityName}'
                
                Waqti:
                    - Coordinates:  
                    ({waqtiLocationData?.Latitude} / {waqtiLocationData?.Longitude})
                    - Timezone:     
                    '{waqtiLocationData?.TimezoneName}'
                """;
    }

    public string GetPrayerTimeConfigDisplayText(DynamicProfile profile)
    {
        var outputText = new StringBuilder();

        foreach (KeyValuePair<ETimeSection, List<ETimeType>> item in timeTypeAttributeService.SectionToTimeTypes)
        {
            ETimeSection section = item.Key;
            outputText.AppendLine(section.ToString());

            foreach (ETimeType timeType in item.Value)
            {
                if (!timeTypeAttributeService.ConfigurableTypes.Contains(timeType))
                {
                    continue;
                }

                GenericSettingConfiguration config = GetTimeConfig(profile, timeType);

                // profiles which were saved before a time type was introduced have no config for it
                if (config is null)
                {
                    continue;
                }

                outputText.Append(Environment.NewLine);
                outputText.Append($"- {timeType} mit {config.Source}");
                if (config is MuwaqqitDegreeCalculationConfiguration degreeConfig)
                {
                    outputText.Append($" ({degreeConfig.Degree}°)");
                }
                else if (config is WaqtiCalculationConfiguration waqtiConfig)
                {
                    outputText.Append(waqtiConfig.Degree is double waqtiDegree
                        ? $" ({waqtiConfig.Method}, {waqtiDegree}°)"
                        : $" ({waqtiConfig.Method})");
                }

                if (config.MinuteAdjustment != 0)
                {
                    outputText.Append($", {config.MinuteAdjustment:N0}min");
                }
            }

            outputText.AppendLine();
            outputText.AppendLine();
        }

        return outputText.ToString();
    }

    public List<GenericSettingConfiguration> GetActiveComplexTimeConfigs(DynamicProfile profile)
    {
        return [.. timeTypeAttributeService
            .ComplexTypes
            .Select(x => GetTimeConfig(profile, x))
            .Where(config =>
                config is GenericSettingConfiguration
                {
                    Source: not EDynamicPrayerTimeProviderType.None,
                    IsTimeShown: true
                })];
    }

    public Task<MosqueProfile> CreateNewMosqueProfile(EMosquePrayerTimeProviderType providerType, string externalID, string? mosqueName, CancellationToken cancellationToken)
    {
        string profileName = string.IsNullOrWhiteSpace(mosqueName)
            ? $"{providerType}: {externalID}"
            : mosqueName.Trim();

        return profileRepository.CreateNewMosqueProfile(providerType, externalID, profileName, cancellationToken);
    }

    public DateTimeZone GetDateTimeZone(Profile profile)
    {
        return profile switch
        {
            DynamicProfile dynamicProfile => DateTimeZoneProviders.Tzdb[dynamicProfile.PlaceInfo.TimezoneInfo.Name],
            // MosqueProfiles don't have place infos and the calculation only relies on LocalDateTime part of zonedDateTime
            MosqueProfile => DateTimeZone.Utc,
            _ => throw new InvalidOperationException($"Unknown profile type '{profile.GetType().FullName}'")
        };
    }

    public async Task ChangeProfileName(Profile profile, string newProfileName, CancellationToken cancellationToken)
    {
        await profileRepository.ChangeProfileName(profile, newProfileName, cancellationToken).ConfigureAwait(false);
        BumpProfileVersion(profile.ID);
    }

    public ZonedDateTime GetCurrentZonedDateTime(DynamicProfile profile)
    {
        return systemInfoService.GetCurrentInstant()
                .InZone(DateTimeZoneProviders.Tzdb[profile.PlaceInfo.TimezoneInfo.Name]);
    }
}
