using MvvmHelpers;
using PrayerTimeEngine.Code.Domain;
using PrayerTimeEngine.Code.Interfaces;
using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Domain.Models;
using System.Windows.Input;
using PrayerTimeEngine.Code.Presentation.Service.Navigation;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PropertyChanged;

namespace PrayerTimeEngine.Code.Presentation.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class MainPageViewModel : BaseViewModel
    {
        public MainPageViewModel(
            IPrayerTimeCalculationService prayerTimeCalculator,
            INavigationService navigationService,
            PrayerTimesConfigurationStorage prayerTimesConfigurationStorage)
        {
            _navigationService = navigationService;
            _prayerTimeCalculationService = prayerTimeCalculator;
            _prayerTimesConfigurationStorage = prayerTimesConfigurationStorage;
        }

        #region fields

        private readonly INavigationService _navigationService;
        private readonly IPrayerTimeCalculationService _prayerTimeCalculationService;
        private readonly PrayerTimesConfigurationStorage _prayerTimesConfigurationStorage;

        #endregion fields

        #region properties

        public PrayerTimesBundle Prayers { get; private set; }
        public bool IsLoading { get; private set; }
        public bool IsNotLoading => !IsLoading;

        #endregion properties

        #region ICommand

        public ICommand GoToSettingsPageCommand 
            => new Command<EPrayerTime>(
                async (EPrayerTime prayerTime) => 
                {
                    await _navigationService.NavigateTo<SettingsMainPageViewModel>(prayerTime);
                });

        public ICommand LoadPrayerTimesButton_ClickCommand
            => new Command(
                async () =>
                {
                    try
                    {
                        IsLoading = true;
                        Profile profile = (await _prayerTimesConfigurationStorage.GetProfiles()).First();
                        Prayers = await _prayerTimeCalculationService.ExecuteAsync(profile, DateTime.Today);
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                });

        #endregion ICommand
    }
}
