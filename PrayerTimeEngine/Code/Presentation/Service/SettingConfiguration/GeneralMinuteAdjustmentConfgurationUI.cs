using PrayerTimeEngine.Code.Common;
using PrayerTimeEngine.Code.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Presentation.Service.SettingConfiguration
{
    public class GeneralMinuteAdjustmentConfgurationUI : ISettingConfigurationUI
    {
        public Picker MinuteAdjustmentPicker = new Picker
        {
            ItemsSource = new List<int> { -30, -25, -20, -15, -10, -5 },
            SelectedIndex = 0,
        };

        public void AssignConfigurationValues(BaseCalculationConfiguration calculationConfiguration)
        {
            if (calculationConfiguration is not GeneralMinuteAdjustmentConfguration config)
            {
                return;
            }

            this.MinuteAdjustmentPicker.SelectedItem = config.MinuteAdjustment;
        }

        public BaseCalculationConfiguration GetSettings(int minuteAdjustment)
        {
            return new GeneralMinuteAdjustmentConfguration(minuteAdjustment);
        }

        public StackLayout GetUI()
        {
            StackLayout stackLayout = new StackLayout();
            stackLayout.Children.Add(MinuteAdjustmentPicker);
            return stackLayout;
        }
    }
}
