using MethodTimer;
using PrayerTimeEngine.Code.Presentation.View;
using PrayerTimeEngine.Presentation.ViewModel;
using UraniumUI.Material.Controls;

namespace PrayerTimeEngine.Views;

public partial class SettingsHandlerPage : ContentPage
{
    private readonly SettingsHandlerPageViewModel _viewModel;

    public SettingsHandlerPage(SettingsHandlerPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        viewModel.Initialized += setUpTabPages;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // for ViewModel to handle setting saving
        Task.Run(onDisappearingForAllSettingContentPages);
    }

    [Time]
    private async Task onDisappearingForAllSettingContentPages()
    {
        foreach (SettingsContentPageViewModel contentPageViewModel in _viewModel.SettingsContentPages.Select(x => x.ViewModel))
        {
            await contentPageViewModel.OnDisappearing();
        }
    }

    private void setUpTabPages()
    {
        foreach (SettingsContentPage settingContentPages in _viewModel.SettingsContentPages)
        {
            var tabItem = new TabItem
            {
                Content = new ContentView
                {
                    Content = settingContentPages.Content,
                    BindingContext = settingContentPages.ViewModel
                },
                BindingContext = settingContentPages.ViewModel,
            };

            tabItem.SetBinding(TabItem.TitleProperty, nameof(settingContentPages.ViewModel.TabTitle));
            tabView.Items.Add(tabItem);
        }
    }
}

