﻿using PrayerTimeEngine.Presentation.ViewModel;
using PrayerTimeEngine.Views;

namespace PrayerTimeEngine.Presentation.Service.Navigation
{
    public interface INavigationService
    {
        Task NavigateTo<TViewModel>(params object[] parameter) where TViewModel : CustomBaseViewModel;
        Task NavigateBack();
    }

    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Type> _mapping;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _mapping = [];

            CreatePageViewModelMappings();
        }

        private void CreatePageViewModelMappings()
        {
            _mapping.Add(typeof(MainPageViewModel), typeof(MainPage));
            _mapping.Add(typeof(SettingsHandlerPageViewModel), typeof(SettingsHandlerPage));
            _mapping.Add(typeof(DatabaseTablesPageViewModel), typeof(DatabaseTablesPage));
        }

        public async Task NavigateTo<TViewModel>(params object[] parameter) where TViewModel : CustomBaseViewModel
        {
            var targetType = _mapping[typeof(TViewModel)];

            if (Application.Current.MainPage is NavigationPage navigationPage)
            {
                var page = (Page)_serviceProvider.GetRequiredService(targetType);

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

}
