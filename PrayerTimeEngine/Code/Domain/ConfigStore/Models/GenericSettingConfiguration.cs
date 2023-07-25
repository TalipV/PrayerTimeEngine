using PrayerTimeEngine.Code.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.ConfigStore.Models
{
    public class GenericSettingConfiguration : BaseCalculationConfiguration
    {
        public GenericSettingConfiguration(int minuteAdjustment, bool isTimeShown = true, ECalculationSource calculationSource = ECalculationSource.None)
            : base(minuteAdjustment, isTimeShown)
        {
            Source = calculationSource;
        }

        public override ECalculationSource Source { get; }
    }
}
