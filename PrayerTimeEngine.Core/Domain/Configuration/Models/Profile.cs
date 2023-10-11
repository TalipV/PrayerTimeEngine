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

        [NotMapped]
        public Dictionary<ETimeType, GenericSettingConfiguration> Configurations { get; init; } = new();
        [NotMapped]
        public Dictionary<ECalculationSource, BaseLocationData> LocationDataByCalculationSource { get; init; } = new();

        public GenericSettingConfiguration GetConfiguration(ETimeType timeType)
        {
            if (!Configurations.TryGetValue(timeType, out GenericSettingConfiguration calculationConfiguration)
                || calculationConfiguration == null)
            {
                Configurations[timeType] = calculationConfiguration = new GenericSettingConfiguration { TimeType = timeType };
            }

            return calculationConfiguration;
        }
    }
}
