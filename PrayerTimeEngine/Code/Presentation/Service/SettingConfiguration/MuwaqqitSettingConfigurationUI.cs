using PrayerTimeEngine.Code.Domain.Muwaqqit.Models;
using PrayerTimeEngine.Code.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Presentation.Service.SettingConfiguration
{

    public class MuwaqqitSettingConfigurationUI : ISettingConfigurationUI
    {
        public virtual void AssignConfigurationValues(BaseCalculationConfiguration calculationConfiguration)
        {
            
        }

        public virtual BaseCalculationConfiguration GetSettings(int minuteAdjustment)
        {
            return new MuwaqqitCalculationConfiguration(minuteAdjustment, 0M, 0M, "");
        }

        public virtual StackLayout GetUI()
        {
            var stackLayout = new StackLayout();
            //stackLayout.Children.Add(Label1);
            return stackLayout;
        }
    }
}
