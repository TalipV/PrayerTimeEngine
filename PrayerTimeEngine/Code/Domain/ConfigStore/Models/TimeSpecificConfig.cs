using PrayerTimeEngine.Code.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.ConfigStore.Models
{
    public class TimeSpecificConfig
    {
        public int ID { get; set; }
        public int ProfileID { get; set; }
        public EPrayerTime PrayerTime { get; set; }
        public EPrayerTimeEvent PrayerTimeEvent { get; set; }
        public BaseCalculationConfiguration CalculationConfiguration { get; set; }
    }
}
