using PrayerTimeEngine.Code.Presentation.View;
using PrayerTimeEngine.Presentation.ViewModel;

namespace PrayerTimeEngine.Views;

public partial class SettingsHandlerPage : ContentPage
{
    private readonly SettingsHandlerPageViewModel _viewModel;

    public SettingsHandlerPage(SettingsHandlerPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // for ViewModel to handle setting saving
        Task.Run(async () =>
        {
            foreach (SettingsContentPageViewModel contentPageViewModel in _viewModel.SettingsContentPages.Select(x => x.ViewModel))
            {
                await contentPageViewModel.OnDisappearing();
            }
        });
    }
}

