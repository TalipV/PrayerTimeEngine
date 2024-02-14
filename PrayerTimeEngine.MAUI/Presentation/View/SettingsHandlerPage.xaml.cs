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
#if WINDOWS
        //TabbedPage windowsTabBar = new TabbedPage
        //{
        //    Title = "Windows Tab View Placeholder"
        //};

        //// Assuming placeholderView is the ContentView where the tab should be added
        //this.tabViewSection = windowsTabBar;

        //foreach (SettingsContentPage settingContentPages in _viewModel.SettingsContentPages)
        //{
        //    ShellContent tab = new ShellContent
        //    {
        //        BindingContext = settingContentPages.ViewModel,
        //    };

        //    tab.SetBinding(Tab.TitleProperty, nameof(SettingsContentPageViewModel.TabTitle));

        //    tab.Items.Add(settingContentPages);
        //    windowsTabBar.Items.Add(tab);
        //}
#else
        var tabView = new DevExpress.Maui.Controls.TabView()
        {
            HeaderPanelContentAlignment = DevExpress.Maui.Controls.HeaderContentAlignment.Start
        };
        this.tabViewSection.Content = tabView;

        foreach (SettingsContentPage settingContentPages in _viewModel.SettingsContentPages)
        {
            addSettingsContentPageAsTab(tabView, settingContentPages);
        }
#endif
    }

#if !WINDOWS
    private void addSettingsContentPageAsTab(DevExpress.Maui.Controls.TabView tabView, SettingsContentPage settingContentPages)
    {
        var tabViewItem = new DevExpress.Maui.Controls.TabViewItem
        {
            Content = new ContentView
            {
                Content = settingContentPages.Content,
                BindingContext = settingContentPages.ViewModel
            },
            BindingContext = settingContentPages.ViewModel,
            HeaderFontSize = 14
        };

        tabViewItem.SetBinding(DevExpress.Maui.Controls.TabViewItem.HeaderTextProperty, nameof(SettingsContentPageViewModel.TabTitle));
        tabView.Items.Add(tabViewItem);
    }
#endif
}

