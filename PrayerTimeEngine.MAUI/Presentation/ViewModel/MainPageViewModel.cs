using MvvmHelpers;
using System.Windows.Input;
using PropertyChanged;
using static System.Runtime.InteropServices.JavaScript.JSType;
using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Model;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Presentation.Service.Navigation;

namespace PrayerTimeEngine.Presentation.ViewModel
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

        public Profile CurrentProfile
        {
            get
            {
                return _prayerTimesConfigurationStorage.GetProfiles().GetAwaiter().GetResult().First();
            }
        }

        public bool ShowFajrGhalas { get; set; }
        public bool ShowFajrRedness { get; set; }
        public bool ShowDuhaQuarter { get; set; }
        public bool ShowMithlayn { get; set; }
        public bool ShowKaraha { get; set; }
        public bool ShowIshtibaq { get; set; }
        public bool ShowMaghribSufficientTime { get; set; }
        public bool ShowOneThird { get; set; }
        public bool ShowTwoThird { get; set; }
        public bool ShowMidnight { get; set; }

        #endregion properties

        #region ICommand

        public ICommand GoToSettingsPageCommand
            => new Command<EPrayerType>(
                async (prayerTime) =>
                {
                    await _navigationService.NavigateTo<SettingsHandlerPageViewModel>(prayerTime);
                });

        public ICommand LoadPrayerTimesButton_ClickCommand
            => new Command(
                async () =>
                {
                    try
                    {
                        IsLoading = true;
                        Prayers = await _prayerTimeCalculationService.ExecuteAsync(CurrentProfile, DateTime.Today);
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                });

        #endregion ICommand

        #region public methods

        public void OnAppearing()
        {
            setValuesYo();
        }

        #endregion public methods

        #region private methods 

        private void setValuesYo()
        {
            ShowFajrGhalas = IsCalculationShown(ETimeType.FajrGhalas);
            ShowFajrRedness = IsCalculationShown(ETimeType.FajrKaraha);
            ShowDuhaQuarter = IsCalculationShown(ETimeType.DuhaQuarterOfDay);
            ShowMithlayn = IsCalculationShown(ETimeType.AsrMithlayn);
            ShowKaraha = IsCalculationShown(ETimeType.AsrKaraha);
            ShowMaghribSufficientTime = IsCalculationShown(ETimeType.MaghribSufficientTime);
            ShowIshtibaq = IsCalculationShown(ETimeType.MaghribIshtibaq);
            ShowOneThird = IsCalculationShown(ETimeType.IshaFirstThird);
            ShowTwoThird = IsCalculationShown(ETimeType.IshaSecondThird);
            ShowMidnight = IsCalculationShown(ETimeType.IshaMidnight);

            OnPropertyChanged();
        }

        private bool IsCalculationShown(ETimeType timeData)
        {
            if (!CurrentProfile.Configurations.TryGetValue(timeData, out BaseCalculationConfiguration config) || config == null)
            {
                return true;
            }

            return config.IsTimeShown;
        }

        #endregion private methods
    }
}
