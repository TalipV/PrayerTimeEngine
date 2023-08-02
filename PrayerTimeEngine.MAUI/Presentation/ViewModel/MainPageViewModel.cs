using MvvmHelpers;
using System.Windows.Input;
using PropertyChanged;
using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Model;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Presentation.Service.Navigation;
using System.Linq;

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
                // only show data when 
                if (Prayers == null || Prayers.AllPrayerTimes.Any(x => x.Start == null || x.End == null))
                {
                    return null;
                }

                DateTime dateTime = DateTime.Now;

                return Prayers.AllPrayerTimes.FirstOrDefault(x => x.Start <= dateTime && dateTime <= x.End)
                    ?? Prayers.AllPrayerTimes.OrderBy(x => x.Start).FirstOrDefault(x => x.Start > dateTime);
            }
        }

        public PrayerTimesBundle Prayers { get; private set; }

        public DateTime? LastUpdated { get; private set; }

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
                LastUpdated = DateTime.Now;
                IsLoading = false;
            }
        }

        #endregion ICommand

        #region public methods

        public async void OnActualAppearing()
        {
            showHideSpecificTimes();
            await loadPrayerTimes();
        }

        #endregion public methods

        #region private methods 

        private void showHideSpecificTimes()
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
