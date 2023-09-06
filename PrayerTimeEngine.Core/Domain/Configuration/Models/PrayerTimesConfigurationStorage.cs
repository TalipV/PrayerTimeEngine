using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;

namespace PrayerTimeEngine.Core.Domain.Configuration.Models
{
    public class PrayerTimesConfigurationStorage
    {
        IConfigStoreService _configStoreService;

        public PrayerTimesConfigurationStorage(IConfigStoreService configStoreService)
        {
            _configStoreService = configStoreService;
        }

        List<Profile> _profiles = null;

        public async Task<List<Profile>> GetProfiles()
        {
            if (_profiles == null)
            {
                _profiles = await _configStoreService.GetProfiles();

                if (_profiles.Count == 0)
                {
                    _profiles.Add(getDummyProfile());
                }
            }

            return _profiles;
        }

        public async Task<GenericSettingConfiguration> GetConfiguration(ETimeType timeType)
        {
            Profile profile = (await GetProfiles()).First();
            return profile.GetConfiguration(timeType);
        }

        private static Profile getDummyProfile()
        {
            return new Profile
            {
                ID = 1,
                Name = "Standard-Profil",
                LocationName = "Innsbruck",
                SequenceNo = 1,
                LocationDataByCalculationSource =
                {
                    [ECalculationSource.Muwaqqit] =
                        new MuwaqqitLocationData
                        {
                            Latitude = 47.2803835M,
                            Longitude = 11.41337M,
                            TimezoneName = "Europe/Vienna"
                        },
                    [ECalculationSource.Fazilet] =
                        new FaziletLocationData
                        {
                            CountryName = "Avusturya",
                            CityName = "Innsbruck"
                        },
                    [ECalculationSource.Semerkand] =
                        new SemerkandLocationData
                        {
                            CountryName = "Avusturya",
                            CityName = "Innsbruck",
                            TimezoneName = "Europe/Vienna"
                        },
                },
                Configurations =
                    new List<GenericSettingConfiguration>()
                    {
                        new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.FajrStart }, // späteste Berechnung
                        new GenericSettingConfiguration { Source = ECalculationSource.Semerkand, TimeType = ETimeType.FajrEnd }, // früheste Berechnung
                        new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Degree = -8.5 }, // Grobe Einschätzung anhand von Sichtung  
                        new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Degree = -4.0 }, // ### keine Erfahrung
                        new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.DuhaStart, Degree = 5.0 }, // von Gebetszeiten-Hoca, späteste
                        new GenericSettingConfiguration { TimeType = ETimeType.DuhaEnd, MinuteAdjustment = -25 }, // ### keine Erfahrung
                        new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.DhuhrStart }, // späteste Berechnung
                        new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, TimeType = ETimeType.DhuhrEnd }, // früheste Berechnung
                        new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.AsrStart }, // späteste Berechnung
                        new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, TimeType = ETimeType.AsrEnd }, // früheste Berechnung
                        new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, TimeType = ETimeType.AsrMithlayn },
                        new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.AsrKaraha, Degree = 5.0 }, // von Gebetszeiten-Hoca, früheste
                        new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.MaghribStart },
                        new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribEnd, Degree = -15.0 }, // ### keine Erfahrung, aber Sicherheitsabstand
                        new GenericSettingConfiguration { TimeType = ETimeType.MaghribSufficientTime, MinuteAdjustment = 20 },
                        new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribIshtibaq, Degree = -10.0 }, // von Gebetszeiten-Hoca
                        new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.IshaStart },
                        new GenericSettingConfiguration { Source = ECalculationSource.Semerkand, TimeType = ETimeType.IshaEnd },
                    }.ToDictionary(x => x.TimeType)
            };
        }
    }
}
