using MetroLog.Maui;

namespace PrayerTimeEngine;

public partial class App : Application
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

    protected override void OnResume()
    {
        base.OnResume();
        Resumed?.Invoke();
    }
}
