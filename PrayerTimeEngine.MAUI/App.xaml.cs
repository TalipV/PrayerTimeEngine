using MethodTimer;
using MetroLog.Maui;
using Microsoft.EntityFrameworkCore;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain;

namespace PrayerTimeEngine;

public partial class App : Application
{
    public event Action Resumed;

    public App(
        IServiceProvider serviceProvider,
        ConcurrentDataLoader concurrentDataLoader)
    {
        // start concurrent loading as soon as possible
        concurrentDataLoader.InitiateConcurrentDataLoad();
        
        InitializeComponent();

        MethodTimeLogger.logger = serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<App>>();

        MainPage = new NavigationPage(serviceProvider.GetService<MainPage>());

        // initialize log page access
        LogController.InitializeNavigation(
            MainPage.Navigation.PushModalAsync,
            MainPage.Navigation.PopModalAsync);

        // slightly slowls down startup
        // DevExpress.Maui.Editors.Initializer.Init();
        // DevExpress.Maui.Controls.Initializer.Init();
    }

    protected override void OnResume()
    {
        base.OnResume();
        Resumed?.Invoke();
    }
}
