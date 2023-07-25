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

    private void updateConfigurationSettingUI()
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

    protected override void OnDisappearing()
    {
        ViewModel.OnDisappearing();
    }
}