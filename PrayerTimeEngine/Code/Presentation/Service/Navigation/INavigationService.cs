
/* Unmerged change from project 'PrayerTimeEngine (net7.0-android)'
Before:
using PrayerTimeEngine.Code.Presentation.ViewModel;
After:
using PrayerTimeEngine;
using PrayerTimeEngine.Code;
using PrayerTimeEngine.Code.Presentation.Presentation;
using PrayerTimeEngine.Code.Presentation.Service;
using PrayerTimeEngine.Code.Presentation.ViewModel;
*/
using PrayerTimeEngine.Code.Presentation.ViewModel;
using PrayerTimeEngine.Views;

/* Unmerged change from project 'PrayerTimeEngine (net7.0-android)'
Before:
using System.Threading.Tasks;
After:
using System.Threading.Tasks;
using PrayerTimeEngine;
using PrayerTimeEngine.Code;
using PrayerTimeEngine.Code.Presentation;
using PrayerTimeEngine.Code.Presentation.Service.Navigation;
*/

namespace PrayerTimeEngine.Code.Presentation.Service.Navigation
{
    public interface INavigationService
    {
        Task NavigateTo<TViewModel>(object parameter = null) where TViewModel : CustomBaseViewModel;
        Task NavigateBack();
    }


    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Type> _mapping;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _mapping = new Dictionary<Type, Type>();

            CreatePageViewModelMappings();
        }

        private void CreatePageViewModelMappings()
        {
            _mapping.Add(typeof(MainPageViewModel), typeof(MainPage));
            _mapping.Add(typeof(SettingsMainPageViewModel), typeof(SettingsMainPage));
        }


        public async Task NavigateTo<TViewModel>(object parameter = null) where TViewModel : CustomBaseViewModel
        {
            var targetType = _mapping[typeof(TViewModel)];

            if (Application.Current.MainPage is NavigationPage navigationPage)
            {
                var page = (Page)_serviceProvider.GetRequiredService(targetType);

                if (page.BindingContext is TViewModel viewModel)
                {
                    viewModel.Initialize(parameter);
                }

                await navigationPage.PushAsync(page);
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
                await navigationPage.PopAsync();
            }
        }
    }

}
