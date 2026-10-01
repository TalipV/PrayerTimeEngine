using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.Pages.Settings.SettingsContent.Custom;

[AddINotifyPropertyChangedInterface]
public class MuwaqqitDegreeSettingConfigurationViewModel : ISettingConfigurationViewModel
{
    public MuwaqqitDegreeSettingConfigurationViewModel(ETimeType timeType)
    {
        TimeType = timeType;
        DegreeItemsSource = GetItemSource(TimeType);
        SelectedDegree = DegreeItemsSource.First();
    }

    public double SelectedDegree { get; set; }
    public IReadOnlyCollection<double> DegreeItemsSource { get; set; }

    public ETimeType TimeType { get; init; }

    public GenericSettingConfiguration BuildSetting(int minuteAdjustment, bool isTimeShown)
    {
        return
            new MuwaqqitDegreeCalculationConfiguration
            {
                TimeType = TimeType,
                MinuteAdjustment = minuteAdjustment,
                Degree = SelectedDegree,
                IsTimeShown = isTimeShown
            };
    }

    private StackLayout _stackLayout = null;

    public IView GetUI()
    {
        if (_stackLayout is null)
        {
            _stackLayout = new StackLayout
            {
                BindingContext = this
            };

            _stackLayout.Children.Add(new Label { Text = "Degree", TextColor = AppColors.Text });

            var picker = new Picker
            {
                ItemsSource = DegreeItemsSource.ToList()
            };
            picker.SetBinding(Picker.SelectedItemProperty, new Binding(nameof(SelectedDegree)));
            _stackLayout.Children.Add(picker);
        }

        return _stackLayout;
    }

    public void AssignSettingValues(GenericSettingConfiguration configuration)
    {
        if (configuration is not MuwaqqitDegreeCalculationConfiguration muwaqqitConfig)
        {
            throw new ArgumentException($"{nameof(configuration)} is not of type {nameof(MuwaqqitDegreeCalculationConfiguration)}");
        }

        DegreeItemsSource = GetItemSource(TimeType);
        SelectedDegree = muwaqqitConfig.Degree;
    }

    private static IReadOnlyCollection<double> GetItemSource(ETimeType timeType)
    {
        if (timeType == ETimeType.DuhaStart || timeType == ETimeType.AsrKaraha)
        {
            return SettingsContentPageViewModel.MODERATE_SELECTABLE_DEGREES_POSITIVE;
        }
        else if (timeType == ETimeType.MaghribEnd
            || timeType == ETimeType.IshaStart
            || timeType == ETimeType.FajrStart
            || timeType == ETimeType.IshaEnd)
        {
            return SettingsContentPageViewModel.FAJR_ISHA_SELECTABLE_DEGREES;
        }
        else if (timeType == ETimeType.FajrGhalas || timeType == ETimeType.FajrKaraha)
        {
            return SettingsContentPageViewModel.MODERATE_SELECTABLE_DEGREES_NEGATIVE;
        }
        else if (timeType == ETimeType.MaghribIshtibak)
        {
            return SettingsContentPageViewModel.ISHTIBAK_SELECTABLE_DEGREES;
        }
        else
        {
            throw new NotImplementedException($"{timeType} not implemented for degree selection values.");
        }
    }
}
