using PrayerTimeEngine.Code.Common.Enums;
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

        ViewModel.OnCustomSettingUIReady += () => updateConfigurationSettingUI();
    }

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is not Picker picker 
            || picker.SelectedItem is not ECalculationSource calculationSource)
        {
            return;
        }

        ViewModel.OnCalculationSourceChanged(calculationSource);
    }

    private void updateConfigurationSettingUI()
    {
        ConfigurableUIContainer.Children.Clear();

        if (ViewModel.CustomSettingConfigurationUI == null)
        {
            return;
        }

        StackLayout newUI = ViewModel.CustomSettingConfigurationUI.GetUI();
        ConfigurableUIContainer.Children.Add(newUI);
    }

    protected override void OnDisappearing()
    {
        ViewModel.OnDisappearing();
    }
}