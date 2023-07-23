using PrayerTimeEngine.Code.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Presentation.Service.SettingConfiguration
{
    public interface ISettingConfigurationUI
    {
        StackLayout GetUI();
        BaseCalculationConfiguration GetSettings(int minuteAdjustment);
        void AssignConfigurationValues(BaseCalculationConfiguration calculationConfiguration);
    }
}
