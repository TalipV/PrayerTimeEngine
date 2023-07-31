using MvvmHelpers;
using System.Windows.Input;
using PropertyChanged;
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

        public PrayerTime DisplayPrayerTime
        {
            get
            {
                if (Prayers == null)
                {
                    return null;
                }

                DateTime dateTime = DateTime.Now;

                if (Prayers.Fajr.Start != null && Prayers.Fajr.End != null)
                {
                    if (Prayers.Fajr.Start < dateTime && dateTime < Prayers.Fajr.End)
                    {
                        return Prayers.Fajr;
                    }
                }
                if (Prayers.Duha.Start != null && Prayers.Duha.End != null)
                {
                    if (Prayers.Duha.Start < dateTime && dateTime < Prayers.Duha.End)
                    {
                        return Prayers.Duha;
                    }
                }
                if (Prayers.Dhuhr.Start != null && Prayers.Dhuhr.End != null)
                {
                    if (Prayers.Dhuhr.Start < dateTime && dateTime < Prayers.Dhuhr.End)
                    {
                        return Prayers.Dhuhr;
                    }
                }
                if (Prayers.Asr.Start != null && Prayers.Asr.End != null)
                {
                    if (Prayers.Asr.Start < dateTime && dateTime < Prayers.Asr.End)
                    {
                        return Prayers.Asr;
                    }
                }
                if (Prayers.Maghrib.Start != null && Prayers.Maghrib.End != null)
                {
                    if (Prayers.Maghrib.Start < dateTime && dateTime < Prayers.Maghrib.End)
                    {
                        return Prayers.Maghrib;
                    }
                }
                if (Prayers.Isha.Start != null && Prayers.Isha.End != null)
                {
                    if (Prayers.Isha.Start < dateTime && dateTime < Prayers.Isha.End)
                    {
                        return Prayers.Isha;
                    }
                }

                return null;
            }
        }

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

        public event Action OnAfterLoadingPrayerTimes_EventTrigger = delegate { };

        public ICommand LoadPrayerTimesButton_ClickCommand
            => new Command(
                async () =>
                {
                    await loadPrayerTimes();
                });

        private async Task loadPrayerTimes()
        {
            try
            {
                IsLoading = true;
                Prayers = await _prayerTimeCalculationService.ExecuteAsync(CurrentProfile, DateTime.Today);
                OnAfterLoadingPrayerTimes_EventTrigger.Invoke();
            }
            finally
            {
                IsLoading = false;
            }
        }

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
            if (!CurrentProfile.Configurations.TryGetValue(timeData, out GenericSettingConfiguration config) || config == null)
            {
                return true;
            }

            return config.IsTimeShown;
        }

        #endregion private methods
    }
}
