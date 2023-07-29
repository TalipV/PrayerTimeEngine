using PrayerTimeEngine.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Domain.ConfigStore.Models
{
    public class GenericSettingConfiguration : BaseCalculationConfiguration
    {
        public GenericSettingConfiguration(ETimeType timeType, int minuteAdjustment = 0, ECalculationSource calculationSource = ECalculationSource.None, bool isTimeShown = true)
            : base(minuteAdjustment, isTimeShown)
        {
            TimeType = timeType;
            Source = calculationSource;
        }

        public override ECalculationSource Source { get; }
    }
}
