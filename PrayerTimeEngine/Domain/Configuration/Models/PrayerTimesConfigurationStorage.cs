using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Model;
using PrayerTimeEngine.Domain.ConfigStore.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Domain.Calculators.Semerkand.Models;

namespace PrayerTimeEngine.Domain.ConfigStore.Models
{
    public class PrayerTimesConfigurationStorage
    {
        IConfigStoreService _configStoreService;

        public PrayerTimesConfigurationStorage(IConfigStoreService configStoreService)
        {
            _configStoreService = configStoreService;
        }

        public const string TIMEZONE = "Europe/Vienna";

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

        public GenericSettingConfiguration GetConfiguration(ETimeType timeType)
        {
            Profile profile = GetProfiles().GetAwaiter().GetResult().First();
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
                            Longitude = 11.41337M
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
                            CityName = "Innsbruck"
                        },
                },
                Configurations =
                    new Dictionary<ETimeType, GenericSettingConfiguration>()
                    {
                        [ETimeType.FajrStart] = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrStart, MinuteAdjustment = 0, Degree = -12.0 },
                        [ETimeType.FajrEnd] = new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, MinuteAdjustment = 0, Source = ECalculationSource.Muwaqqit },
                        [ETimeType.FajrGhalas] = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrGhalas, MinuteAdjustment = 0, Degree = -7.5 },
                        [ETimeType.FajrKaraha] = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrKaraha, MinuteAdjustment = 0, Degree = -4.5 },
                        [ETimeType.DuhaStart] = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.DuhaStart, MinuteAdjustment = 0, Degree = 3.5 },
                        [ETimeType.DuhaEnd] = new GenericSettingConfiguration { TimeType = ETimeType.DuhaEnd, MinuteAdjustment = -20 },
                        [ETimeType.DhuhrStart] = new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, MinuteAdjustment = 0, Source = ECalculationSource.Muwaqqit },
                        [ETimeType.DhuhrEnd] = new GenericSettingConfiguration { TimeType = ETimeType.DhuhrEnd, MinuteAdjustment = 0, Source = ECalculationSource.Muwaqqit },
                        [ETimeType.AsrStart] = new GenericSettingConfiguration { TimeType = ETimeType.AsrStart, MinuteAdjustment = 0, Source = ECalculationSource.Muwaqqit },
                        [ETimeType.AsrEnd] = new GenericSettingConfiguration { TimeType = ETimeType.AsrEnd, MinuteAdjustment = 0, Source = ECalculationSource.Muwaqqit },
                        [ETimeType.AsrMithlayn] = new GenericSettingConfiguration { TimeType = ETimeType.AsrMithlayn, MinuteAdjustment = 0, Source = ECalculationSource.Muwaqqit },
                        [ETimeType.AsrKaraha] = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.AsrKaraha, MinuteAdjustment = 0, Degree = 4.5 },
                        [ETimeType.MaghribStart] = new GenericSettingConfiguration { TimeType = ETimeType.MaghribStart, MinuteAdjustment = 0, Source = ECalculationSource.Muwaqqit },
                        [ETimeType.MaghribEnd] = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribEnd, MinuteAdjustment = 0, Degree = -12.0 },
                        [ETimeType.MaghribSufficientTime] = new GenericSettingConfiguration { TimeType = ETimeType.MaghribSufficientTime, MinuteAdjustment = 20 },
                        [ETimeType.MaghribIshtibaq] = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribIshtibaq, MinuteAdjustment = 0, Degree = -8 },
                        [ETimeType.IshaStart] = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.IshaStart, MinuteAdjustment = 0, Degree = -15.5 },
                        [ETimeType.IshaEnd] = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.IshaEnd, MinuteAdjustment = 0, Degree = -15.0 },
                    }
            };
        }
    }
}
