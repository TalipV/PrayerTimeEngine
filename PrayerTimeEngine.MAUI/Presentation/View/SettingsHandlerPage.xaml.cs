using PrayerTimeEngine.Code.Presentation.View;
using PrayerTimeEngine.Presentation.ViewModel;

namespace PrayerTimeEngine.Views;

public partial class SettingsHandlerPage : TabbedPage
{
    public SettingsHandlerPage(SettingsHandlerPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        viewModel.Initialized += () => setUpTabPages(viewModel);
    }

    private void setUpTabPages(SettingsHandlerPageViewModel viewModel)
    {
        foreach (SettingsContentPage settingContentPages in viewModel.SettingsContentPages)
        {
            settingContentPages.SetBinding(TitleProperty, "TabTitle");
            this.Children.Add(settingContentPages);
        }
    }
}

