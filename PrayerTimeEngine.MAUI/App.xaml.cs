using MethodTimer;
using MetroLog.Maui;
using Microsoft.EntityFrameworkCore;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Data.EntityFramework;

namespace PrayerTimeEngine;

public partial class App : Application
{
    public event Action Resumed;

    public App(
        IServiceProvider serviceProvider,
        AppDbContext dbContext,
        MainPage page)
    {
        InitializeComponent();
        MethodTimeLogger.logger = serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<App>>();

        additionalAppSetup(page, dbContext);
    }

    [Time]
    private void additionalAppSetup(MainPage page, AppDbContext dbContext)
    {
        // first page
        MainPage = new NavigationPage(page);

        // initialize log page access
        LogController.InitializeNavigation(
            MainPage.Navigation.PushModalAsync,
            MainPage.Navigation.PopModalAsync);

        // slightly slowls down startup
        // DevExpress.Maui.Editors.Initializer.Init();
        // DevExpress.Maui.Controls.Initializer.Init();

        dbContext.Database.Migrate();
    }

    protected override void OnResume()
    {
        base.OnResume();
        Resumed?.Invoke();
    }
}
