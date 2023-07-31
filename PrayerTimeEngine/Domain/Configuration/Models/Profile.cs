using PrayerTimeEngine.Common.Enum;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PrayerTimeEngine.Domain.ConfigStore.Models
{
    public class Profile
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int SequenceNo { get; set; }
        public Dictionary<ETimeType, GenericSettingConfiguration> Configurations { get; set; }

        public List<ILocationConfig> LocationConfigurations { get; set; }

        public GenericSettingConfiguration GetConfiguration(ETimeType timeType)
        {
            if (!Configurations.TryGetValue(timeType, out GenericSettingConfiguration calculationConfiguration)
                || calculationConfiguration == null)
            {
                Configurations[timeType] = calculationConfiguration = new GenericSettingConfiguration(timeType);
            }

            return calculationConfiguration;
        }
    }
}
