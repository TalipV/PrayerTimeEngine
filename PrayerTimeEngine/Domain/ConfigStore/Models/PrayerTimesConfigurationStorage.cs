using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Model;
using PrayerTimeEngine.Domain.ConfigStore.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;

namespace PrayerTimeEngine.Domain.ConfigStore.Models
{
    public class PrayerTimesConfigurationStorage
    {
        IConfigStoreService _configStoreService;

        public PrayerTimesConfigurationStorage(IConfigStoreService configStoreService)
        {
            _configStoreService = configStoreService;
        }

        public PrayerTimeLocation Location { get; set; }

        public const decimal INNSBRUCK_LATITUDE = 47.2803835M;
        public const decimal INNSBRUCK_LONGITUDE = 11.41337M;

        public const string COUNTRY_NAME = "Avusturya";
        public const string CITY_NAME = "Innsbruck";
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

        public BaseCalculationConfiguration GetConfiguration(ETimeType timeType)
        {
            Profile profile = GetProfiles().GetAwaiter().GetResult().First();
            return profile.GetConfiguration(timeType);
        }

        private Profile getDummyProfile()
        {
            return new Profile
            {
                ID = 1,
                Name = "Standard-Profil",
                SequenceNo = 1,
                Configurations =
                    new Dictionary<ETimeType, BaseCalculationConfiguration>()
                    {
                        [ETimeType.FajrStart] = new MuwaqqitDegreeCalculationConfiguration(ETimeType.FajrStart, 0, -12.0),
                        [ETimeType.FajrEnd] = new GenericSettingConfiguration(ETimeType.FajrEnd, 0, ECalculationSource.Muwaqqit),
                        [ETimeType.FajrGhalas] = new MuwaqqitDegreeCalculationConfiguration(ETimeType.FajrGhalas, 0, -7.5),
                        [ETimeType.FajrKaraha] = new MuwaqqitDegreeCalculationConfiguration(ETimeType.FajrKaraha, 0, -4.5),

                        [ETimeType.DuhaStart] = new MuwaqqitDegreeCalculationConfiguration(ETimeType.DuhaStart, 0, 3.5),
                        [ETimeType.DuhaEnd] = new GenericSettingConfiguration(ETimeType.DuhaEnd, -20),

                        [ETimeType.DhuhrStart] = new GenericSettingConfiguration(ETimeType.DhuhrStart, 0, ECalculationSource.Muwaqqit),
                        [ETimeType.DhuhrEnd] = new GenericSettingConfiguration(ETimeType.DhuhrEnd, 0, ECalculationSource.Muwaqqit),

                        [ETimeType.AsrStart] = new GenericSettingConfiguration(ETimeType.AsrStart, 0, ECalculationSource.Muwaqqit),
                        [ETimeType.AsrEnd] = new GenericSettingConfiguration(ETimeType.AsrEnd, 0, ECalculationSource.Muwaqqit),
                        [ETimeType.AsrMithlayn] = new GenericSettingConfiguration(ETimeType.AsrMithlayn, 0, ECalculationSource.Muwaqqit),
                        [ETimeType.AsrKaraha] = new MuwaqqitDegreeCalculationConfiguration(ETimeType.AsrKaraha, 0, 4.5),

                        [ETimeType.MaghribStart] = new GenericSettingConfiguration(ETimeType.MaghribStart, 0, ECalculationSource.Muwaqqit),
                        [ETimeType.MaghribEnd] = new MuwaqqitDegreeCalculationConfiguration(ETimeType.MaghribEnd, 0, -12.0),
                        [ETimeType.MaghribSufficientTime] = new GenericSettingConfiguration(ETimeType.MaghribSufficientTime, 20),
                        [ETimeType.MaghribIshtibaq] = new MuwaqqitDegreeCalculationConfiguration(ETimeType.MaghribIshtibaq, 0, -8),

                        [ETimeType.IshaStart] = new MuwaqqitDegreeCalculationConfiguration(ETimeType.IshaStart, 0, -15.5),
                        [ETimeType.IshaEnd] = new MuwaqqitDegreeCalculationConfiguration(ETimeType.IshaEnd, 0, -15.0),
                    },
            };
        }
    }
}
