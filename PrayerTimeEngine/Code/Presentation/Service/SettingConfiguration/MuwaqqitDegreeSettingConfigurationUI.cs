using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.Domain.Muwaqqit.Models;
using PrayerTimeEngine.Code.Interfaces;
using PrayerTimeEngine.Code.Presentation.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Presentation.Service.SettingConfiguration
{
    public class MuwaqqitDegreeSettingConfigurationUI : MuwaqqitSettingConfigurationUI
    {
        public Label DegreePickerLabel = new Label
        {
            Text = "Degree",
        };

        public Picker DegreePicker = new Picker
        {
            SelectedIndex = 0,
        };

        private (EPrayerTime, EPrayerTimeEvent) _prayerTimeWithEvent;

        private List<double> getItemSource()
        {
            if (_prayerTimeWithEvent.Item2 == EPrayerTimeEvent.Start || _prayerTimeWithEvent.Item2 == EPrayerTimeEvent.End)
            {
                return SettingsContentPageViewModel.FAJR_ISHA_SELECTABLE_DEGREES;
            }
            else
            {
                return SettingsContentPageViewModel.MODERATE_SELECTABLE_DEGREES;
            }
        }

        public MuwaqqitDegreeSettingConfigurationUI((EPrayerTime, EPrayerTimeEvent) PrayerTimeWithEvent)
        {
            _prayerTimeWithEvent = PrayerTimeWithEvent;
            DegreePicker.ItemsSource = getItemSource();
        }

        public override void AssignConfigurationValues(BaseCalculationConfiguration calculationConfiguration)
        {
            base.AssignConfigurationValues(calculationConfiguration);

            if (calculationConfiguration is not MuwaqqitDegreeCalculationConfiguration muwaqqitDegreeConfig)
            {
                return;
            }

            this.DegreePicker.SelectedItem = muwaqqitDegreeConfig.Degree;
        }

        public override BaseCalculationConfiguration GetSettings(int minuteAdjustment)
        {
            MuwaqqitCalculationConfiguration muwaqqitBaseCalculationConfig = base.GetSettings(minuteAdjustment) as MuwaqqitCalculationConfiguration;
            double? degreeValue = DegreePicker.SelectedItem as double?;

            return new MuwaqqitDegreeCalculationConfiguration(
                    muwaqqitBaseCalculationConfig.MinuteAdjustment,
                    muwaqqitBaseCalculationConfig.Longitude,
                    muwaqqitBaseCalculationConfig.Latitude,
                    muwaqqitBaseCalculationConfig.Timezone,
                    degreeValue ?? MuwaqqitDegreeCalculationConfiguration.DegreePrayerTimeEvents[_prayerTimeWithEvent]
                );
        }

        public override StackLayout GetUI()
        {
            StackLayout baseLayout = base.GetUI();
            baseLayout.Children.Add(DegreePickerLabel);
            baseLayout.Children.Add(DegreePicker);
            return baseLayout;
        }
    }
}
