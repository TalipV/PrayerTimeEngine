using MetroLog.Maui;
using PrayerTimeEngine.Core.Data.EntityFramework;

namespace PrayerTimeEngine;

public partial class App : Application
{
    public event Action Resumed;

    public App(MainPage page, AppDbContext dbContext)
    {
        InitializeComponent();

        MainPage = new NavigationPage(page);

        // Initialize the database
        dbContext.Database.EnsureCreated();

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
