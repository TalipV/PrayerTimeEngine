using PrayerTimeEngine.Presentation.ViewModel;

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
        this.CalculationSourcePickerView.SetBinding(Picker.ItemsSourceProperty, nameof(SettingsContentPageViewModel.CalculationSources), BindingMode.Default);
        this.CalculationSourcePickerLabelView.SetBinding(Label.IsVisibleProperty, nameof(SettingsContentPageViewModel.ShowCalculationSourcePicker), BindingMode.Default);
        this.CalculationSourcePickerView.SetBinding(Picker.SelectedItemProperty, nameof(SettingsContentPageViewModel.SelectedCalculationSource), BindingMode.TwoWay);
        this.CalculationSourcePickerView.SetBinding(Picker.IsVisibleProperty, nameof(SettingsContentPageViewModel.ShowCalculationSourcePicker), BindingMode.Default);

        this.MinuteAdjustmentPickerView.SetBinding(Picker.ItemsSourceProperty, nameof(SettingsContentPageViewModel.MinuteAdjustments), BindingMode.Default);
        this.MinuteAdjustmentPickerView.SetBinding(Picker.SelectedItemProperty, nameof(SettingsContentPageViewModel.SelectedMinuteAdjustment), BindingMode.TwoWay);

        this.IsTimeShownCheckBoxLabel.SetBinding(Label.IsVisibleProperty, nameof(SettingsContentPageViewModel.IsTimeShownCheckBoxVisible), BindingMode.Default);
        this.IsTimeShownCheckBox.SetBinding(CheckBox.IsCheckedProperty, nameof(SettingsContentPageViewModel.IsTimeShown), BindingMode.TwoWay);
        this.IsTimeShownCheckBox.SetBinding(CheckBox.IsVisibleProperty, nameof(SettingsContentPageViewModel.IsTimeShownCheckBoxVisible), BindingMode.Default);
    }
}