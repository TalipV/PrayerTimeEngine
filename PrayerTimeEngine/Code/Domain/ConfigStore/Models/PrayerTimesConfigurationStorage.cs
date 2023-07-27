using PrayerTimeEngine.Code.Domain.Models;
using PrayerTimeEngine.Code.Domain.ConfigStore.Interfaces;
using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Domain.Calculator.Muwaqqit.Models;

namespace PrayerTimeEngine.Code.Domain.ConfigStore.Models
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
            Profile profile = this.GetProfiles().GetAwaiter().GetResult().First();
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
                    new Dictionary<(EPrayerTime, EPrayerTimeEvent), BaseCalculationConfiguration>()
                    {
                        [(EPrayerTime.Fajr, EPrayerTimeEvent.Start)] = new MuwaqqitDegreeCalculationConfiguration(0, -12.0),
                        [(EPrayerTime.Fajr, EPrayerTimeEvent.End)] = new GenericSettingConfiguration(0, ECalculationSource.Muwaqqit),
                        [(EPrayerTime.Fajr, EPrayerTimeEvent.Fajr_Fadilah)] = new MuwaqqitDegreeCalculationConfiguration(0, -8),
                        [(EPrayerTime.Fajr, EPrayerTimeEvent.Fajr_Karaha)] = new MuwaqqitDegreeCalculationConfiguration(0, -4.5),

                        [(EPrayerTime.Duha, EPrayerTimeEvent.Start)] = new MuwaqqitDegreeCalculationConfiguration(0, 4.5),
                        [(EPrayerTime.Duha, EPrayerTimeEvent.End)] = new GenericSettingConfiguration(-20),

                        [(EPrayerTime.Dhuhr, EPrayerTimeEvent.Start)] = new GenericSettingConfiguration(0, ECalculationSource.Muwaqqit),
                        [(EPrayerTime.Dhuhr, EPrayerTimeEvent.End)] = new GenericSettingConfiguration(0, ECalculationSource.Muwaqqit),

                        [(EPrayerTime.Asr, EPrayerTimeEvent.Start)] = new GenericSettingConfiguration(0, ECalculationSource.Muwaqqit),
                        [(EPrayerTime.Asr, EPrayerTimeEvent.End)] = new GenericSettingConfiguration(0, ECalculationSource.Muwaqqit),
                        [(EPrayerTime.Asr, EPrayerTimeEvent.AsrMithlayn)] = new GenericSettingConfiguration(0, ECalculationSource.Muwaqqit),
                        [(EPrayerTime.Asr, EPrayerTimeEvent.Asr_Karaha)] = new MuwaqqitDegreeCalculationConfiguration(0, 4.5),

                        [(EPrayerTime.Maghrib, EPrayerTimeEvent.Start)] = new GenericSettingConfiguration(0, ECalculationSource.Muwaqqit),
                        [(EPrayerTime.Maghrib, EPrayerTimeEvent.End)] = new MuwaqqitDegreeCalculationConfiguration(0, -12.0),
                        [(EPrayerTime.Maghrib, EPrayerTimeEvent.IshtibaqAnNujum)] = new MuwaqqitDegreeCalculationConfiguration(0, -8),

                        [(EPrayerTime.Isha, EPrayerTimeEvent.Start)] = new MuwaqqitDegreeCalculationConfiguration(0, -15.5),
                        [(EPrayerTime.Isha, EPrayerTimeEvent.End)] = new MuwaqqitDegreeCalculationConfiguration(0, -15.0),
                    },
            };
        }
    }
}
