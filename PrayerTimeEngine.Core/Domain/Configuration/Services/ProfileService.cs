using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Core.Domain.Configuration.Services
{
    public class ProfileService(
            IProfileDBAccess profileDBAccess,
            TimeTypeAttributeService timeTypeAttributeService
        ) : IProfileService
    {
        public async Task<List<Profile>> GetProfiles()
        {
            List<Profile> profiles = await profileDBAccess.GetProfiles().ConfigureAwait(false);

            if (profiles.Count == 0)
            {
                profiles.Add(getDefaultProfile());
                await SaveProfile(profiles[0]);
            }

            return profiles;
        }

        public async Task SaveProfile(Profile profile)
        {
            await profileDBAccess.SaveProfile(profile).ConfigureAwait(false);
        }

        public GenericSettingConfiguration GetTimeConfig(Profile profile, ETimeType timeType)
        {
            return profile.TimeConfigs.FirstOrDefault(x => x.TimeType == timeType)?.CalculationConfiguration;
        }

        public BaseLocationData GetLocationConfig(Profile profile, ECalculationSource calculationSource)
        {
            return profile.LocationConfigs.FirstOrDefault(x => x.CalculationSource == calculationSource)?.LocationData;
        }

        public async Task UpdateLocationConfig(
            Profile profile,
            string locationName,
            List<(ECalculationSource CalculationSource, BaseLocationData LocationData)> locationDataByCalculationSource)
        {
            await profileDBAccess.UpdateLocationConfig(profile, locationName, locationDataByCalculationSource);
        }

        public async Task UpdateTimeConfig(Profile profile, ETimeType timeType, GenericSettingConfiguration settings)
        {
            await profileDBAccess.UpdateTimeConfig(profile, timeType, settings);
        }

        public string GetLocationDataDisplayText(Profile profile)
        {
            if (profile == null)
                return string.Empty;

            MuwaqqitLocationData muwaqqitLocationData = this.GetLocationConfig(profile, ECalculationSource.Muwaqqit) as MuwaqqitLocationData;
            FaziletLocationData faziletLocationData = this.GetLocationConfig(profile, ECalculationSource.Fazilet) as FaziletLocationData;
            SemerkandLocationData semerkandLocationData = this.GetLocationConfig(profile, ECalculationSource.Semerkand) as SemerkandLocationData;

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
            string outputText = string.Empty;

            foreach (KeyValuePair<EPrayerType, List<ETimeType>> item in timeTypeAttributeService.PrayerTypeToTimeTypes)
            {
                EPrayerType prayerType = item.Key;
                outputText += prayerType.ToString();

                foreach (ETimeType timeType in item.Value)
                {
                    if (!timeTypeAttributeService.ConfigurableTypes.Contains(timeType))
                        continue;

                    GenericSettingConfiguration config = this.GetTimeConfig(profile, timeType);

                    outputText += Environment.NewLine;
                    outputText += $"- {timeType} mit {config.Source}";
                    if (config is MuwaqqitDegreeCalculationConfiguration degreeConfig)
                    {
                        outputText += $" ({degreeConfig.Degree}°)";
                    }
                }

                outputText += Environment.NewLine;
                outputText += Environment.NewLine;
            }

            return outputText;
        }

        public bool EqualsFullProfile(Profile profile1, Profile profile2)
        {
            if (profile1 == null)
                throw new ArgumentNullException(nameof(profile1));
            if (profile2 == null)
                throw new ArgumentNullException(nameof(profile2));

            if (profile1.TimeConfigs.Count != profile2.TimeConfigs.Count)
                return false;
            if (profile1.LocationConfigs.Count != profile2.LocationConfigs.Count)
                return false;

            if (!profile1.Equals(profile2))
                return false;

            var profile1TimeConfigs = profile1.TimeConfigs.OrderBy(x => x.ID);
            var profile2TimeConfigs = profile2.TimeConfigs.OrderBy(x => x.ID);
            for(int i = 0; i < profile1.TimeConfigs.Count; i++)
            {
                if (!profile1TimeConfigs.ElementAt(i).Equals(profile2TimeConfigs.ElementAt(i)))
                    return false;
            }

            var profile1LocationConfigs = profile1.LocationConfigs.OrderBy(x => x.ID);
            var profile2LocationConfigs = profile2.LocationConfigs.OrderBy(x => x.ID);
            for(int i = 0; i < profile1.LocationConfigs.Count; i++)
            {
                if (!profile1LocationConfigs.ElementAt(i).Equals(profile2LocationConfigs.ElementAt(i)))
                    return false;
            }

            return true;
        }

        private static Profile getDefaultProfile()
        {
            Profile profile = new Profile
            {
                ID = 1,
                Name = "Standard-Profil",
                LocationName = "Innsbruck",
                SequenceNo = 1,
            };

            profile.LocationConfigs =
                new List<ProfileLocationConfig>
                {
                    new ProfileLocationConfig
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
                    new ProfileLocationConfig
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
                    new ProfileLocationConfig
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
                };

            profile.TimeConfigs =
                new List<ProfileTimeConfig>
                {
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.FajrStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // späteste Berechnung
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.FajrStart }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.FajrEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // früheste Berechnung
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Semerkand, TimeType = ETimeType.FajrEnd }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.FajrGhalas,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // Grobe Einschätzung anhand von Sichtung  
                        CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Degree = -8.5 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.FajrKaraha,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // ### keine Erfahrung
                        CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Degree = -4.0 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.DuhaStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // von Gebetszeiten-Hoca, späteste
                        CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.DuhaStart, Degree = 5.0 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.DuhaEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // ### keine Erfahrung
                        CalculationConfiguration = new GenericSettingConfiguration { TimeType = ETimeType.DuhaEnd, MinuteAdjustment = -25 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.DhuhrStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // späteste Berechnung
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.DhuhrStart }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.DhuhrEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // früheste Berechnung
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, TimeType = ETimeType.DhuhrEnd }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.AsrStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // späteste Berechnung
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.AsrStart }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.AsrEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // früheste Berechnung
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, TimeType = ETimeType.AsrEnd }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.AsrMithlayn,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, TimeType = ETimeType.AsrMithlayn }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.AsrKaraha,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // von Gebetszeiten-Hoca, früheste
                        CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.AsrKaraha, Degree = 5.0 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.MaghribStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // späteste Berechnung
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.MaghribStart }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.MaghribEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // ### keine Erfahrung, aber Sicherheitsabstand
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Semerkand, TimeType = ETimeType.MaghribEnd }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.MaghribSufficientTime,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { TimeType = ETimeType.MaghribSufficientTime, MinuteAdjustment = 20 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.MaghribIshtibaq,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // von Gebetszeiten-Hoca
                        CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribIshtibaq, Degree = -10.0 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.IshaStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // späteste Berechnung
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.IshaStart }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.IshaEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        // früheste Berechnung
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Semerkand, TimeType = ETimeType.IshaEnd }
                    }
                };

            return profile;
        }
    }
}
