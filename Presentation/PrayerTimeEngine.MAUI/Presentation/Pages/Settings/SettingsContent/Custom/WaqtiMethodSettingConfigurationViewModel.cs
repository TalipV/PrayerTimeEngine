using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models;
using PropertyChanged;
using PrayerTimeEngine.Presentation.Localization;

namespace PrayerTimeEngine.Presentation.Pages.Settings.SettingsContent.Custom;

/// <summary>
/// Lets the user pick the Waqti base calculation method for a time and, for the times Waqti
/// derives from a sun angle (Ghalas, Karaha, Ishtibak), the angle like with Muwaqqit.
/// </summary>
[AddINotifyPropertyChangedInterface]
public class WaqtiMethodSettingConfigurationViewModel : ISettingConfigurationViewModel
{
    public WaqtiMethodSettingConfigurationViewModel(ETimeType timeType)
    {
        TimeType = timeType;
        HasDegree = WaqtiCalculationConfiguration.DEGREE_TIME_TYPES.Contains(timeType);

        if (HasDegree)
        {
            // the same selectable angles as with Muwaqqit
            DegreeItemsSource = MuwaqqitDegreeSettingConfigurationViewModel.GetItemSource(timeType);
            SelectedDegree = DegreeItemsSource.First();
        }
    }

    public string SelectedMethod { get; set; } = WaqtiMethods.DEFAULT_METHOD;
    public IReadOnlyList<string> MethodItemsSource { get; } = WaqtiMethods.ALL_METHODS;

    public bool HasDegree { get; }
    public double SelectedDegree { get; set; }
    public IReadOnlyCollection<double> DegreeItemsSource { get; } = [];

    public ETimeType TimeType { get; }

    public GenericSettingConfiguration BuildSetting(int minuteAdjustment, bool isTimeShown)
    {
        return
            new WaqtiCalculationConfiguration
            {
                TimeType = TimeType,
                MinuteAdjustment = minuteAdjustment,
                Method = SelectedMethod,
                Degree = HasDegree ? SelectedDegree : null,
                IsTimeShown = isTimeShown
            };
    }

    private StackLayout? _stackLayout;

    public IView GetUI()
    {
        if (_stackLayout is null)
        {
            _stackLayout = new StackLayout
            {
                BindingContext = this
            };

            _stackLayout.Children.Add(new Label { Text = AppResources.Settings_Method, TextColor = AppColors.Text });

            var methodPicker = new Picker
            {
                ItemsSource = MethodItemsSource.ToList()
            };
            methodPicker.SetBinding(Picker.SelectedItemProperty, new Binding(nameof(SelectedMethod)));
            _stackLayout.Children.Add(methodPicker);

            if (HasDegree)
            {
                _stackLayout.Children.Add(new Label { Text = AppResources.Settings_Degree, TextColor = AppColors.Text });

                var degreePicker = new Picker
                {
                    ItemsSource = DegreeItemsSource.ToList()
                };
                degreePicker.SetBinding(Picker.SelectedItemProperty, new Binding(nameof(SelectedDegree)));
                _stackLayout.Children.Add(degreePicker);
            }
        }

        return _stackLayout;
    }

    public void AssignSettingValues(GenericSettingConfiguration configuration)
    {
        // a time switched to Waqti just now still has the configuration of its previous source
        var waqtiConfiguration = configuration as WaqtiCalculationConfiguration;

        SelectedMethod =
            waqtiConfiguration?.Method is string method && MethodItemsSource.Contains(method)
                ? method
                : WaqtiMethods.DEFAULT_METHOD;

        if (HasDegree)
        {
            SelectedDegree =
                waqtiConfiguration?.Degree is double degree && DegreeItemsSource.Contains(degree)
                    ? degree
                    : DegreeItemsSource.First();
        }
    }
}
