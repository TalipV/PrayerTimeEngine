using MetroLog.Maui;
using PrayerTimeEngine.Core.Data.SQLite;

namespace PrayerTimeEngine;

public partial class App : Application
{
    public event Action Resumed;

    public App(MainPage page, ISQLiteDB sqliteDB)
    {
        InitializeComponent();

        MainPage = new NavigationPage(page);

        // Initialize the database
        sqliteDB.InitializeDatabase();

        LogController.InitializeNavigation(
            MainPage.Navigation.PushModalAsync,
            MainPage.Navigation.PopModalAsync);
    }

    protected override void OnResume()
    {
        base.OnResume();
        Resumed?.Invoke();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);

        const int newWidth = 440;
        const int newHeight = 800;

        window.Width = newWidth;
        window.Height = newHeight;

        return window;
    }
}
