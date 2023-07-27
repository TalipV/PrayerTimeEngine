using PrayerTimeEngine.Code.Presentation.ViewModel;

namespace PrayerTimeEngine.Code.Presentation.View;

public partial class SettingsContentPage : ContentPage
{
    public SettingsContentPageViewModel ViewModel { get; private set; }

    public SettingsContentPage(SettingsContentPageViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;

        ViewModel.OnInitializeCustomUI_EventTrigger += () => onInitializeCustomUI_EventTrigger();
        ViewModel.OnViewModelInitialize_EventTrigger += onViewModelInitialize_EventTrigger;
    }

    private void onViewModelInitialize_EventTrigger()
    {
        addDataBindings();
    }

    private void onInitializeCustomUI_EventTrigger()
    {
        configureCustomUISection();
    }

    protected override void OnDisappearing()
    {
        // for ViewModel to handle setting saving
        ViewModel.OnDisappearing();
    }

    private void configureCustomUISection()
    {
        ConfigurableUIContainer.Children.Clear();

        if (ViewModel.CustomSettingConfigurationViewModel == null)
        {
            return;
        }

        if (ViewModel.CustomSettingConfigurationViewModel.GetUI() is IView customSettingConfiguration)
        {
            ConfigurableUIContainer.Children.Add(customSettingConfiguration);
        }
    }

    private void addDataBindings()
    {
        this.CalculationSourcePickerLabelView
            .SetBinding(
                Label.IsVisibleProperty,
                nameof(SettingsContentPageViewModel.ShowCalculationSourcePicker),
                BindingMode.Default);
        this.CalculationSourcePickerView
            .SetBinding(
                Picker.SelectedItemProperty,
                nameof(SettingsContentPageViewModel.SelectedCalculationSource),
                BindingMode.TwoWay);
        this.CalculationSourcePickerView
            .SetBinding(
                Picker.ItemsSourceProperty,
                nameof(SettingsContentPageViewModel.CalculationSources),
                BindingMode.Default);
        this.CalculationSourcePickerView
            .SetBinding(
                Picker.IsVisibleProperty,
                nameof(SettingsContentPageViewModel.ShowCalculationSourcePicker),
                BindingMode.Default);

        this.MinuteAdjustmentPickerView
            .SetBinding(
                Picker.SelectedItemProperty,
                nameof(SettingsContentPageViewModel.SelectedMinuteAdjustment),
                BindingMode.TwoWay);
        this.MinuteAdjustmentPickerView
            .SetBinding(
                Picker.ItemsSourceProperty,
                nameof(SettingsContentPageViewModel.MinuteAdjustments),
                BindingMode.Default);

        this.IsTimeShownCheckBox
            .SetBinding(
                CheckBox.IsCheckedProperty,
                nameof(SettingsContentPageViewModel.IsTimeShown),
                BindingMode.TwoWay);
        this.IsTimeShownCheckBox
            .SetBinding(
                CheckBox.IsVisibleProperty,
                nameof(SettingsContentPageViewModel.IsTimeShownCheckBoxVisible),
                BindingMode.Default);
    }
}