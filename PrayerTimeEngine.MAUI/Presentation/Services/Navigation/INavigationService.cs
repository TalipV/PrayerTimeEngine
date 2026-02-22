using PrayerTimeEngine.Presentation.Pages.DatabaseTables;
using PrayerTimeEngine.Presentation.Pages.Main;
using PrayerTimeEngine.Presentation.Pages.Settings.SettingsHandler;

namespace PrayerTimeEngine.Presentation.Services.Navigation;

public interface INavigationService
{
    Task NavigateTo<TViewModel>(params object[] parameter) 
        where TViewModel : CustomBaseViewModel;
    Task NavigateTo<PageType>()
        where PageType : Page;

    Task NavigateBack();
}

public class NavigationService(
        IServiceProvider serviceProvider
    ) : INavigationService
{
    private readonly Dictionary<Type, Type> _mapping =
        new()
        {
            [typeof(MainPageViewModel)] = typeof(MainPage),
            [typeof(SettingsHandlerPageViewModel)] = typeof(SettingsHandlerPage),
            [typeof(DatabaseTablesPageViewModel)] = typeof(DatabaseTablesPage),
        };

    public async Task NavigateTo<TViewModel>(params object[] parameter) 
        where TViewModel : CustomBaseViewModel
    {
        var targetType = _mapping[typeof(TViewModel)];

        NavigationPage navigationPage = getNavigationPage();
        var page = (Page)serviceProvider.GetRequiredService(targetType);
        if (page.BindingContext is TViewModel viewModel)
        {
            viewModel.Initialize(parameter);
        }
        await navigationPage.PushAsync(page).ConfigureAwait(false);
    }

    public async Task NavigateTo<PageType>()
        where PageType : Page
    {
        NavigationPage navigationPage = getNavigationPage();
        Page page = serviceProvider.GetRequiredService<PageType>();
        await navigationPage.PushAsync(page).ConfigureAwait(false);
    }

    public async Task NavigateBack()
    {
        if (Application.Current.Windows[0].Page is NavigationPage navigationPage)
        {
            await navigationPage.PopAsync().ConfigureAwait(false);
        }
    }

    private static NavigationPage getNavigationPage()
    {
        if (Application.Current.Windows[0].Page is not NavigationPage navigationPage)
        {
            throw new Exception("Application.Current.MainPage is not of type NavigationPage");
        }

        return navigationPage;
    }
}
