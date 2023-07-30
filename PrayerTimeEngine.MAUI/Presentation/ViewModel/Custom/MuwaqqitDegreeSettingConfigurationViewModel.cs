using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.ViewModel.Custom
{
    [AddINotifyPropertyChangedInterface]
    public class MuwaqqitDegreeSettingConfigurationViewModel : ISettingConfigurationViewModel
    {
        public MuwaqqitDegreeSettingConfigurationViewModel(
            ETimeType timeType,
            double selectedDegree)
        {
            SelectedDegree = selectedDegree;
            TimeType = timeType;
            DegreeItemsSource = getItemSource(TimeType);
        }

        public double SelectedDegree { get; set; }
        public IReadOnlyCollection<double> DegreeItemsSource { get; set; }

        public ETimeType TimeType { get; init; }

        public BaseCalculationConfiguration BuildSetting(int minuteAdjustment, bool isTimeShown)
        {
            return new MuwaqqitDegreeCalculationConfiguration(TimeType, minuteAdjustment, SelectedDegree, isTimeShown);
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
                picker.ItemsSource = DegreeItemsSource.ToList();
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

            DegreeItemsSource = getItemSource(TimeType);
            SelectedDegree = muwaqqitConfig.Degree;
        }

        private IReadOnlyCollection<double> getItemSource(ETimeType timeType)
        {
            if (timeType == ETimeType.DuhaStart || timeType == ETimeType.AsrKaraha)
            {
                return SettingsContentPageViewModel.MODERATE_SELECTABLE_DEGREES_POSITIVE;
            }
            else if (timeType == ETimeType.MaghribEnd
                || timeType == ETimeType.IshaStart
                || timeType == ETimeType.FajrStart
                || timeType == ETimeType.FajrGhalas
                || timeType == ETimeType.FajrKaraha)
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
