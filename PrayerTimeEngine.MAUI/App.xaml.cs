using MetroLog.Maui;
using PrayerTimeEngine.Presentation.View;

namespace PrayerTimeEngine;

public partial class App : Microsoft.Maui.Controls.Application
{
    public event Action Resumed;

    public App(MainPage mainPage)
    {
        InitializeComponent();
        MainPage = new NavigationPage(mainPage);

        // initialize log page access
        LogController.InitializeNavigation(
            MainPage.Navigation.PushModalAsync,
            MainPage.Navigation.PopModalAsync);
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Width = 425;
        window.Height = 800;
        return window;
    }

    protected override void OnResume()
    {
        base.OnResume();
        Resumed?.Invoke();
    }
}
