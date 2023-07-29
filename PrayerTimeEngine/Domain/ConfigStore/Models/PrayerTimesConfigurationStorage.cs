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
                        [ETimeType.FajrStart] = new MuwaqqitDegreeCalculationConfiguration(0, -12.0),
                        [ETimeType.FajrEnd] = new GenericSettingConfiguration(0, ECalculationSource.Muwaqqit),
                        [ETimeType.FajrGhalas] = new MuwaqqitDegreeCalculationConfiguration(0, -8),
                        [ETimeType.FajrKaraha] = new MuwaqqitDegreeCalculationConfiguration(0, -4.5),

                        [ETimeType.DuhaStart] = new MuwaqqitDegreeCalculationConfiguration(0, 4.5),
                        [ETimeType.DuhaEnd] = new GenericSettingConfiguration(-20),

                        [ETimeType.DhuhrStart] = new GenericSettingConfiguration(0, ECalculationSource.Muwaqqit),
                        [ETimeType.DhuhrEnd] = new GenericSettingConfiguration(0, ECalculationSource.Muwaqqit),

                        [ETimeType.AsrStart] = new GenericSettingConfiguration(0, ECalculationSource.Muwaqqit),
                        [ETimeType.AsrEnd] = new GenericSettingConfiguration(0, ECalculationSource.Muwaqqit),
                        [ETimeType.AsrMithlayn] = new GenericSettingConfiguration(0, ECalculationSource.Muwaqqit),
                        [ETimeType.AsrKaraha] = new MuwaqqitDegreeCalculationConfiguration(0, 4.5),

                        [ETimeType.MaghribStart] = new GenericSettingConfiguration(0, ECalculationSource.Muwaqqit),
                        [ETimeType.MaghribEnd] = new MuwaqqitDegreeCalculationConfiguration(0, -12.0),
                        [ETimeType.MaghribIshtibaq] = new MuwaqqitDegreeCalculationConfiguration(0, -8),

                        [ETimeType.IshaStart] = new MuwaqqitDegreeCalculationConfiguration(0, -15.5),
                        [ETimeType.IshaEnd] = new MuwaqqitDegreeCalculationConfiguration(0, -15.0),
                    },
            };
        }
    }
}
