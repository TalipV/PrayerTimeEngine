using PrayerTimeEngine.Code.Common.Enum;

namespace PrayerTimeEngine.Code.Domain.ConfigStore.Models
{
    public class Profile
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int SequenceNo { get; set; }
        public Dictionary<(EPrayerTime, EPrayerTimeEvent), BaseCalculationConfiguration> Configurations { get; set; }

        public List<ILocationConfig> LocationConfigurations { get; set; }
    }
}
