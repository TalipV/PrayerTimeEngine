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
        public ETimeType TimeType { get; set; }
        public BaseCalculationConfiguration CalculationConfiguration { get; set; }
    }
}
