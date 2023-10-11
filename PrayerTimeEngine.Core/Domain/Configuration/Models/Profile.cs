using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Model;
using PropertyChanged;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrayerTimeEngine.Core.Domain.Configuration.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Profile
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string LocationName { get; set; }
        public int SequenceNo { get; set; }

        public ICollection<ProfileTimeConfig> TimeConfigs { get; set; }
        public ICollection<ProfileLocationConfig> LocationConfigs { get; set; }

        public GenericSettingConfiguration GetTimeConfig(ETimeType timeType)
        {
            if (TimeConfigs.FirstOrDefault(x => x.TimeType == timeType) is ProfileTimeConfig foundTimeConfig)
            {
                return foundTimeConfig.CalculationConfiguration;
            }

            // fallback, every single ETimeType (and ECalculationSource) should be represented by a config anyway
            ProfileTimeConfig missingTimeConfig = createNewTimeConfig(timeType);
            return missingTimeConfig.CalculationConfiguration;
        }

        public void SetTimeConfig(ETimeType timeType, GenericSettingConfiguration settings)
        {
            if (TimeConfigs.FirstOrDefault(x => x.TimeType == timeType) is ProfileTimeConfig foundTimeConfig)
            {
                TimeConfigs.Remove(foundTimeConfig);
            }

            createNewTimeConfig(timeType, settings);
        }

        private ProfileTimeConfig createNewTimeConfig(ETimeType timeType, GenericSettingConfiguration config = null)
        {
            ProfileTimeConfig missingTimeConfig =
                new ProfileTimeConfig
                {
                    TimeType = timeType,
                    ProfileID = this.ID,
                    Profile = this,
                    CalculationConfiguration = config ?? new GenericSettingConfiguration { TimeType = timeType }
                };

            TimeConfigs.Add(missingTimeConfig);
            return missingTimeConfig;
        }

        public BaseLocationData GetLocationConfig(ECalculationSource calculationSource)
        {
            return LocationConfigs.FirstOrDefault(x => x.CalculationSource == calculationSource)?.LocationData;
        }

        public void SetLocationConfig(ECalculationSource calculationSource, BaseLocationData locationConfig)
        {
            if (LocationConfigs.FirstOrDefault(x => x.CalculationSource == calculationSource) is ProfileLocationConfig foundLocationConfig)
            {
                LocationConfigs.Remove(foundLocationConfig);
            }

            createNewLocationConfig(calculationSource, locationConfig);
        }

        private ProfileLocationConfig createNewLocationConfig(ECalculationSource calculationSource, BaseLocationData locationData)
        {
            ProfileLocationConfig missingLocationConfig =
                new ProfileLocationConfig
                {
                    CalculationSource = calculationSource,
                    ProfileID = this.ID,
                    Profile = this,
                    LocationData = locationData
                };

            LocationConfigs.Add(missingLocationConfig);
            return missingLocationConfig;
        }
    }
}
