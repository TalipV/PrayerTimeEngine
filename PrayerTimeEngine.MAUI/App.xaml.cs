using MethodTimer;
using MetroLog.Maui;
using PrayerTimeEngine.Core.Common;

namespace PrayerTimeEngine;

public partial class App : Application
{
    public event Action Resumed;

    public App(MainPage page)
    {
        InitializeComponent();
        additionalAppSetup(page);
    }

    [Time]
    private void additionalAppSetup(MainPage page)
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
    }

    protected override void OnResume()
    {
        base.OnResume();
        Resumed?.Invoke();
    }
}
