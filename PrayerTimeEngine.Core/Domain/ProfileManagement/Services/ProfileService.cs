using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using System.Collections.Concurrent;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using System.Text;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Services;

public class ProfileService(
        IProfileRepository profileRepository,
        IDynamicPrayerTimeProviderFactory dynamicPrayerTimeProviderFactory,
        TimeTypeAttributeService timeTypeAttributeService,
        ILogger<ProfileService> logger
    ) : IProfileService
{
    private readonly ConcurrentDictionary<int, long> _profileVersions = new();

    public long GetProfileVersion(int profileID) => _profileVersions.GetValueOrDefault(profileID, 0L);

    private void bumpProfileVersion(int profileID) => _profileVersions.AddOrUpdate(profileID, 1L, (_, v) => v + 1L);

    public async Task<List<Profile>> GetProfiles(CancellationToken cancellationToken)
    {
        List<Profile> profiles = await profileRepository.GetProfiles(cancellationToken).ConfigureAwait(false);

        if (profiles.Count == 0)
        {
            profiles.Add(getDefaultProfile());
            await SaveProfile(profiles[0], cancellationToken).ConfigureAwait(false);
        }

        return profiles;
    }

    public async Task SaveProfile(Profile profile, CancellationToken cancellationToken)
    {
        await profileRepository.SaveProfile(profile, cancellationToken).ConfigureAwait(false);
        if (profile.ID > 0)
            bumpProfileVersion(profile.ID);
    }

    private static DynamicProfile getDefaultProfile()
    {
        var profile = new DynamicProfile
        {
            ID = 1,
            Name = "Standard-Profil",
            PlaceInfo = new ProfilePlaceInfo
            {
                ExternalID = "343647974",
                Latitude = 47.2803835M,
                Longitude = 11.41337M,
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
                            Latitude = 47.2803835M,
                            Longitude = 11.41337M,
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
                    TimeType = ETimeType.MaghribIshtibaq,
                    ProfileID = profile.ID,
                    Profile = profile,
                    // von Gebetszeiten-Hoca
                    CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribIshtibaq, Degree = -10.0 }
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
        await profileRepository.DeleteProfile(profile, cancellationToken).ConfigureAwait(false);
        bumpProfileVersion(profile.ID);
    }

    public GenericSettingConfiguration GetTimeConfig(DynamicProfile profile, ETimeType timeType)
    {
        return profile.TimeConfigs.FirstOrDefault(x => x.TimeType == timeType)?.CalculationConfiguration;
    }

    public BaseLocationData GetLocationConfig(DynamicProfile profile, EDynamicPrayerTimeProviderType dynamicPrayerTimeProviderType)
    {
        return profile.LocationConfigs.FirstOrDefault(x => x.DynamicPrayerTimeProvider == dynamicPrayerTimeProviderType)?.LocationData;
    }

    public async Task UpdateLocationConfig(
        DynamicProfile profile,
        ProfilePlaceInfo placeInfo,
        CancellationToken cancellationToken)
    {
        var locationDataByDynamicPrayerTimeProvider = new List<(EDynamicPrayerTimeProviderType, BaseLocationData)>();

        foreach (var dynamicPrayerTimeProvider in Enum.GetValues<EDynamicPrayerTimeProviderType>())
        {
            if (dynamicPrayerTimeProvider == EDynamicPrayerTimeProviderType.None)
                continue;

            BaseLocationData locationConfig = null;
            try
            {
                locationConfig = await dynamicPrayerTimeProviderFactory
                    .GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(dynamicPrayerTimeProvider)
                    .GetLocationInfo(placeInfo, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, 
                    "Error while retrieving location info for {DynamicPrayerTimeProviderFullName}", 
                    dynamicPrayerTimeProvider.GetType().FullName);
            }

            if (locationConfig != null)
                locationDataByDynamicPrayerTimeProvider.Add((dynamicPrayerTimeProvider, locationConfig));
        }

        string newProfileName = $"{placeInfo?.City ?? "-"}, {profile.SequenceNo}";

        await profileRepository.UpdateLocationConfig(profile, placeInfo, locationDataByDynamicPrayerTimeProvider, newProfileName, cancellationToken);
        bumpProfileVersion(profile.ID);
    }

    public async Task UpdateTimeConfig(DynamicProfile profile, ETimeType timeType, GenericSettingConfiguration settings, CancellationToken cancellationToken)
    {
        await profileRepository.UpdateTimeConfig(profile, timeType, settings, cancellationToken).ConfigureAwait(false);
        bumpProfileVersion(profile.ID);
    }

    public string GetLocationDataDisplayText(DynamicProfile profile)
    {
        if (profile is null)
            return string.Empty;

        MuwaqqitLocationData muwaqqitLocationData = GetLocationConfig(profile, EDynamicPrayerTimeProviderType.Muwaqqit) as MuwaqqitLocationData;
        FaziletLocationData faziletLocationData = GetLocationConfig(profile, EDynamicPrayerTimeProviderType.Fazilet) as FaziletLocationData;
        SemerkandLocationData semerkandLocationData = GetLocationConfig(profile, EDynamicPrayerTimeProviderType.Semerkand) as SemerkandLocationData;

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
                """;
    }

    public string GetPrayerTimeConfigDisplayText(DynamicProfile profile)
    {
        var outputText = new StringBuilder();

        foreach (KeyValuePair<EPrayerType, List<ETimeType>> item in timeTypeAttributeService.PrayerTypeToTimeTypes)
        {
            EPrayerType prayerType = item.Key;
            outputText.AppendLine(prayerType.ToString());

            foreach (ETimeType timeType in item.Value)
            {
                if (!timeTypeAttributeService.ConfigurableTypes.Contains(timeType))
                    continue;

                GenericSettingConfiguration config = GetTimeConfig(profile, timeType);
                outputText.Append(Environment.NewLine);
                outputText.Append($"- {timeType} mit {config.Source}");
                if (config is MuwaqqitDegreeCalculationConfiguration degreeConfig)
                {
                    outputText.Append($" ({degreeConfig.Degree}°)");
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
        return timeTypeAttributeService
            .ComplexTypes
            .Select(x => GetTimeConfig(profile, x))
            .Where(config =>
                config is GenericSettingConfiguration
                {
                    Source: not EDynamicPrayerTimeProviderType.None,
                    IsTimeShown: true
                })
            .ToList();
    }

    public Task<MosqueProfile> CreateNewMosqueProfile(EMosquePrayerTimeProviderType providerType, string externalID, CancellationToken cancellationToken)
    {
        string profileName = $"{providerType}: {externalID}";

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
        bumpProfileVersion(profile.ID);
    }
}
