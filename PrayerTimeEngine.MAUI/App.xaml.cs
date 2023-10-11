using MetroLog.Maui;

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
}
