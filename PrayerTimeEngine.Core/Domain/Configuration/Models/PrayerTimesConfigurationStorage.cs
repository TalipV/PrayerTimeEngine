using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;

namespace PrayerTimeEngine.Core.Domain.Configuration.Models
{
    public class PrayerTimesConfigurationStorage(
            IConfigStoreService configStoreService
        )
    {
        List<Profile> _profiles = null;

        public async Task<List<Profile>> GetProfiles()
        {
            if (_profiles == null)
            {
                _profiles = await configStoreService.GetProfiles().ConfigureAwait(false);

                if (_profiles.Count == 0)
                {
                    _profiles.Add(getDummyProfile());
                }
            }

            return _profiles;
        }

        public async Task<GenericSettingConfiguration> GetConfiguration(ETimeType timeType)
        {
            Profile profile = (await GetProfiles().ConfigureAwait(false)).First();
            return profile.GetTimeConfig(timeType);
        }

        private static Profile getDummyProfile()
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
                        CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribEnd, Degree = -15.0 }
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
