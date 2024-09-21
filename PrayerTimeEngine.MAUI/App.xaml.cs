using MetroLog.Maui;
using PrayerTimeEngine.Presentation.Pages.Main;

namespace PrayerTimeEngine;

public partial class App : Application
{
    private readonly MainPage _mainPage;
    public event Action Resumed;

    public App(MainPage mainPage)
    {
        InitializeComponent();
        _mainPage = mainPage;
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var page = new NavigationPage(_mainPage);
        var window = new Window(page)
        {
            Width = 425,
            Height = 800
        };

        // initialize log page access
        LogController.InitializeNavigation(
            page.Navigation.PushModalAsync,
            page.Navigation.PopModalAsync);

        return window;
    }

    protected override void OnResume()
    {
        base.OnResume();
        Resumed?.Invoke();
    }
}
