using Castle.Core.Logging;
using MethodTimer;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Presentation.Service;
using PrayerTimeEngine.Presentation.ViewModel;
using UraniumUI.Material.Controls;

namespace PrayerTimeEngine.Presentation.View;

public partial class SettingsHandlerPage : ContentPage
{
    private readonly SettingsHandlerPageViewModel _viewModel;
    private readonly ToastMessageService _toastMessageService;
    private readonly ILogger<SettingsHandlerPage> _logger;
    private readonly TabView _tabView;

    public SettingsHandlerPage(
            SettingsHandlerPageViewModel viewModel, 
            ToastMessageService toastMessageService,
            ILogger<SettingsHandlerPage> logger
        )
    {
        _toastMessageService = toastMessageService;
        _logger = logger;

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
        try
        {
            base.OnDisappearing();

            // for ViewModel to handle setting saving
            await onDisappearingForAllSettingContentPages().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while closing settings page");
            _toastMessageService.ShowError(exception.Message);
        }
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

