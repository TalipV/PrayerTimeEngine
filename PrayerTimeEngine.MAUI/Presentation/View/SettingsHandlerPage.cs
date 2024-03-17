using MethodTimer;
using PrayerTimeEngine.Code.Presentation.View;
using PrayerTimeEngine.Presentation.ViewModel;
using UraniumUI.Material.Controls;

namespace PrayerTimeEngine.Views;

public partial class SettingsHandlerPage : ContentPage
{
    private readonly SettingsHandlerPageViewModel _viewModel;
    private readonly TabView _tabView;

    public SettingsHandlerPage(SettingsHandlerPageViewModel viewModel)
    {
        NavigationPage.SetTitleView(this, new Label
        {
            Text = "Configuration",
            FontAttributes = FontAttributes.Bold,
            FontSize = 20,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        });

        Content = _tabView = new TabView
        {
            TabPlacement = TabViewTabPlacement.Top
        };

        BindingContext = _viewModel = viewModel;

        viewModel.Initialized += () =>
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
                _tabView.Items.Add(tabItem);
            }
        };
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        // for ViewModel to handle setting saving
        await onDisappearingForAllSettingContentPages().ConfigureAwait(false);
    }

    [Time]
    private async Task onDisappearingForAllSettingContentPages()
    {
        foreach (SettingsContentPageViewModel contentPageViewModel in _viewModel.SettingsContentPages.Select(x => x.ViewModel))
        {
            await contentPageViewModel.OnDisappearing().ConfigureAwait(false);
        }
    }
}

