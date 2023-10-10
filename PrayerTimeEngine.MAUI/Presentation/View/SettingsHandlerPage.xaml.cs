using DevExpress.Maui.Controls;
using Microsoft.Maui.Controls;
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

        viewModel.Initialized += setUpTabPages;
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

    private void setUpTabPages()
    {
        foreach (SettingsContentPage settingContentPages in _viewModel.SettingsContentPages)
        {
            var tabViewItem = new TabViewItem
            {
                Content = new ContentView
                {
                    Content = settingContentPages.Content,
                    BindingContext = settingContentPages.ViewModel
                },
                BindingContext = settingContentPages.ViewModel,
                HeaderFontSize = 14
            };
            tabViewItem.SetBinding(TabViewItem.HeaderTextProperty, nameof(SettingsContentPageViewModel.TabTitle));

            this.tabView.Items.Add(tabViewItem);
        }
    }
}

