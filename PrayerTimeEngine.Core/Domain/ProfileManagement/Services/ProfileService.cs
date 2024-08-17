using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using System.Text;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Services
{
    public class ProfileService(
            IProfileDBAccess profileDBAccess,
            TimeTypeAttributeService timeTypeAttributeService
        ) : IProfileService
    {
        public Task<Profile> GetUntrackedReferenceOfProfile(int profileID, CancellationToken cancellationToken)
        {
            return profileDBAccess.GetUntrackedReferenceOfProfile(profileID, cancellationToken);
        }

        public async Task<List<Profile>> GetProfiles(CancellationToken cancellationToken)
        {
            List<Profile> profiles = await profileDBAccess.GetProfiles(cancellationToken).ConfigureAwait(false);

            if (profiles.Count == 0)
            {
                profiles.Add(getDefaultProfile());
                await SaveProfile(profiles[0], cancellationToken).ConfigureAwait(false);
            }

            return profiles;
        }

        public Task SaveProfile(Profile profile, CancellationToken cancellationToken)
        {
            return profileDBAccess.SaveProfile(profile, cancellationToken);
        }

        public GenericSettingConfiguration GetTimeConfig(Profile profile, ETimeType timeType)
        {
            return profile.TimeConfigs.FirstOrDefault(x => x.TimeType == timeType)?.CalculationConfiguration;
        }

        public BaseLocationData GetLocationConfig(Profile profile, ECalculationSource calculationSource)
        {
            return profile.LocationConfigs.FirstOrDefault(x => x.CalculationSource == calculationSource)?.LocationData;
        }

        public Task UpdateLocationConfig(
            Profile profile,
            CompletePlaceInfo placeInfo,
            List<(ECalculationSource CalculationSource, BaseLocationData LocationData)> locationDataByCalculationSource,
            CancellationToken cancellationToken)
        {
            return profileDBAccess.UpdateLocationConfig(profile, placeInfo, locationDataByCalculationSource, cancellationToken);
        }

        public Task UpdateTimeConfig(Profile profile, ETimeType timeType, GenericSettingConfiguration settings, CancellationToken cancellationToken)
        {
            return profileDBAccess.UpdateTimeConfig(profile, timeType, settings, cancellationToken);
        }

        public string GetLocationDataDisplayText(Profile profile)
        {
            if (profile is null)
                return string.Empty;

            MuwaqqitLocationData muwaqqitLocationData = GetLocationConfig(profile, ECalculationSource.Muwaqqit) as MuwaqqitLocationData;
            FaziletLocationData faziletLocationData = GetLocationConfig(profile, ECalculationSource.Fazilet) as FaziletLocationData;
            SemerkandLocationData semerkandLocationData = GetLocationConfig(profile, ECalculationSource.Semerkand) as SemerkandLocationData;

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

        public string GetPrayerTimeConfigDisplayText(Profile profile)
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

        public List<GenericSettingConfiguration> GetActiveComplexTimeConfigs(Profile profile)
        {
            return timeTypeAttributeService
                .ComplexTypes
                .Select(x => GetTimeConfig(profile, x))
                .Where(config =>
                    config is GenericSettingConfiguration
                    {
                        Source: not ECalculationSource.None,
                        IsTimeShown: true
                    })
                .ToList();
        }

        private static Profile getDefaultProfile()
        {
            var profile = new Profile
            {
                ID = 1,
                Name = "Standard-Profil",
                PlaceInfo = new CompletePlaceInfo
                {
                    OrmID = "343647974",
                    Latitude = 47.2803835M,
                    Longitude = 11.41337M,
                    InfoLanguageCode = "de",
                    Country = "Österreich", 
                    City = "Innsbruck", 
                    CityDistrict = "",
                    PostCode = "6020", 
                    Street = "",
                    TimezoneInfo  = new TimezoneInfo
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
                        CalculationSource = ECalculationSource.Muwaqqit,
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
                        CalculationSource = ECalculationSource.Fazilet,
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
                        CalculationSource = ECalculationSource.Semerkand,
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
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.FajrStart }
                    },
                    new()
                    {
                        TimeType = ETimeType.FajrEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // früheste Berechnung
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Semerkand, MinuteAdjustment = -5, TimeType = ETimeType.FajrEnd }
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
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.DhuhrStart }
                    },
                    new()
                    {
                        TimeType = ETimeType.DhuhrEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // früheste Berechnung
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, MinuteAdjustment = -5, TimeType = ETimeType.DhuhrEnd }
                    },
                    new()
                    {
                        TimeType = ETimeType.AsrStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // späteste Berechnung
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.AsrStart }
                    },
                    new()
                    {
                        TimeType = ETimeType.AsrEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // früheste Berechnung
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, MinuteAdjustment = -5, TimeType = ETimeType.AsrEnd }
                    },
                    new()
                    {
                        TimeType = ETimeType.AsrMithlayn,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, TimeType = ETimeType.AsrMithlayn }
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
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.MaghribStart }
                    },
                    new()
                    {
                        TimeType = ETimeType.MaghribEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // ### keine Erfahrung, aber Sicherheitsabstand
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Semerkand, MinuteAdjustment = -5, TimeType = ETimeType.MaghribEnd }
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
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.IshaStart }
                    },
                    new()
                    {
                        TimeType = ETimeType.IshaEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // früheste Berechnung
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Semerkand, TimeType = ETimeType.IshaEnd }
                    }
                ];

            return profile;
        }
    }
}
