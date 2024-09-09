using PrayerTimeEngine.Presentation.Pages.DatabaseTables;
using PrayerTimeEngine.Presentation.Pages.Main;
using PrayerTimeEngine.Presentation.Pages.Settings.SettingsHandler;

namespace PrayerTimeEngine.Presentation.Services.Navigation;

public interface INavigationService
{
    Task NavigateTo<TViewModel>(params object[] parameter) where TViewModel : CustomBaseViewModel;
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

    public async Task NavigateTo<TViewModel>(params object[] parameter) where TViewModel : CustomBaseViewModel
    {
        var targetType = _mapping[typeof(TViewModel)];

        if (Application.Current.MainPage is NavigationPage navigationPage)
        {
            var page = (Page)serviceProvider.GetRequiredService(targetType);

            if (page.BindingContext is TViewModel viewModel)
            {
                viewModel.Initialize(parameter);
            }

            await navigationPage.PushAsync(page).ConfigureAwait(false);
        }
        else
        {
            throw new Exception("Application.Current.MainPage is not of type NavigationPage");
        }
    }


    public async Task NavigateBack()
    {
        if (Application.Current.MainPage is NavigationPage navigationPage)
        {
            await navigationPage.PopAsync().ConfigureAwait(false);
        }
    }
}
