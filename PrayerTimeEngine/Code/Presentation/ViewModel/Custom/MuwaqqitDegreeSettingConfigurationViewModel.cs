using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Domain.Calculator.Muwaqqit.Models;
using PrayerTimeEngine.Code.Domain.ConfigStore;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PropertyChanged;

namespace PrayerTimeEngine.Code.Presentation.ViewModel.Custom
{
    [AddINotifyPropertyChangedInterface]
    public class MuwaqqitDegreeSettingConfigurationViewModel : ISettingConfigurationViewModel
    {
        (EPrayerTime prayerTime, EPrayerTimeEvent prayerTimeEvent) PrayerTimeWithEvent;

        public MuwaqqitDegreeSettingConfigurationViewModel(
            (EPrayerTime prayerTime, EPrayerTimeEvent prayerTimeEvent) prayerTimeWithEvent,
            double selectedDegree)
        {
            SelectedDegree = selectedDegree;
            PrayerTimeWithEvent = prayerTimeWithEvent;
            DegreeItemsSource = getItemSource(PrayerTimeWithEvent);
        }

        public double SelectedDegree { get; set; }
        public List<double> DegreeItemsSource { get; set; }

        public BaseCalculationConfiguration BuildSetting(int minuteAdjustment, bool isTimeShown)
        {
            return new MuwaqqitDegreeCalculationConfiguration(minuteAdjustment, SelectedDegree, isTimeShown);
        }

        private StackLayout stackLayout = null;

        public IView GetUI()
        {
            if (stackLayout == null)
            {
                stackLayout = new StackLayout
                {
                    BindingContext = this
                };

                stackLayout.Children.Add(new Label { Text = "Degree" });

                Picker picker = new Picker();
                picker.ItemsSource = DegreeItemsSource;
                picker.SetBinding(Picker.SelectedItemProperty, new Binding("SelectedDegree"));
                stackLayout.Children.Add(picker);
            }

            return stackLayout;
        }

        public void AssignSettingValues(BaseCalculationConfiguration configuration)
        {
            if (configuration is not MuwaqqitDegreeCalculationConfiguration muwaqqitConfig)
            {
                throw new ArgumentException($"{nameof(configuration)} is not of type {nameof(MuwaqqitDegreeCalculationConfiguration)}");
            }

            DegreeItemsSource = getItemSource(PrayerTimeWithEvent);
            SelectedDegree = muwaqqitConfig.Degree;
        }

        private List<double> getItemSource((EPrayerTime prayerTime, EPrayerTimeEvent prayerTimeEvent) prayerTimeWithEvent)
        {
            if ((prayerTimeWithEvent.prayerTime == EPrayerTime.Duha && prayerTimeWithEvent.prayerTimeEvent == EPrayerTimeEvent.Start)
                || prayerTimeWithEvent.prayerTimeEvent == EPrayerTimeEvent.Asr_Karaha)
            {
                return SettingsContentPageViewModel.MODERATE_SELECTABLE_DEGREES.Select(Math.Abs).ToList();
            }
            else if(prayerTimeWithEvent.prayerTimeEvent == EPrayerTimeEvent.Start || prayerTimeWithEvent.prayerTimeEvent == EPrayerTimeEvent.End)
            {
                return SettingsContentPageViewModel.FAJR_ISHA_SELECTABLE_DEGREES;
            }
            else
            {
                return SettingsContentPageViewModel.MODERATE_SELECTABLE_DEGREES;
            }
        }
    }
}
