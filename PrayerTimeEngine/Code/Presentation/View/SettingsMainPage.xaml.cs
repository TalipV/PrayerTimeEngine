using PrayerTimeEngine.Code.Presentation.View;
using PrayerTimeEngine.Code.Presentation.ViewModel;
using System.ComponentModel.Design;

namespace PrayerTimeEngine.Views;

public partial class SettingsMainPage : TabbedPage
{
    private IServiceProvider _serviceProvider;

    public SettingsMainPage(IServiceProvider serviceProvider, SettingsMainPageViewModel viewModel)
    {
        _serviceProvider = serviceProvider;

        InitializeComponent();
        BindingContext = viewModel;

        viewModel.Initialized += () => setUpTabPages(viewModel);
    }

    private void setUpTabPages(SettingsMainPageViewModel viewModel)
    {
        foreach (SettingsContentPage settingContentPages in viewModel.SettingsContentPages)
        {
            settingContentPages.SetBinding(TitleProperty, "TabTitle");
            this.Children.Add(settingContentPages);
        }
    }
}

