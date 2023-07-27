using PrayerTimeEngine.Code.Common.Enum;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PrayerTimeEngine.Code.Domain.ConfigStore.Models
{
    public class Profile
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int SequenceNo { get; set; }
        public Dictionary<ETimeType, BaseCalculationConfiguration> Configurations { get; set; }

        public List<ILocationConfig> LocationConfigurations { get; set; }

        public BaseCalculationConfiguration GetConfiguration(ETimeType timeType)
        {
            if (!this.Configurations.TryGetValue(timeType, out BaseCalculationConfiguration calculationConfiguration)
                || calculationConfiguration == null)
            {
                this.Configurations[timeType] = calculationConfiguration = new GenericSettingConfiguration();
            }

            return calculationConfiguration;
        }
    }
}
