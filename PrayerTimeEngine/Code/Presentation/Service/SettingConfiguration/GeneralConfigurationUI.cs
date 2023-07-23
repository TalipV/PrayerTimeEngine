using PrayerTimeEngine.Code.Common;
using PrayerTimeEngine.Code.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Presentation.Service.SettingConfiguration
{
    public class GeneralConfigurationUI : ISettingConfigurationUI
    {
        public void AssignConfigurationValues(BaseCalculationConfiguration calculationConfiguration)
        {
            if (calculationConfiguration is not GeneralConfiguration config)
            {
                return;
            }
        }

        public BaseCalculationConfiguration GetSettings(int minuteAdjustment)
        {
            return new GeneralConfiguration(minuteAdjustment);
        }

        public StackLayout GetUI()
        {
            StackLayout stackLayout = new StackLayout();
            return stackLayout;
        }
    }
}
