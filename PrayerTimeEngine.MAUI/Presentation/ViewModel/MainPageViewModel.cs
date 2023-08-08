using MvvmHelpers;
using System.Windows.Input;
using PropertyChanged;
using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Model;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Presentation.Service.Navigation;
using System.Linq;
using PrayerTimeEngine.Domain.NominatimLocation.Interfaces;
using PrayerTimeEngine.Domain.LocationService.Models;
using System.Globalization;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Domain.Calculators.Semerkand.Models;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Extensions.Logging;
using MetroLog.Maui;

namespace PrayerTimeEngine.Presentation.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class MainPageViewModel : LogController
    {
        public MainPageViewModel(
            IPrayerTimeCalculationService prayerTimeCalculator,
            IPlaceService placeService,
            INavigationService navigationService,
            PrayerTimesConfigurationStorage prayerTimesConfigurationStorage,
            ILogger<MainPageViewModel> logger)
        {
            _prayerTimeCalculationService = prayerTimeCalculator;
            _placeService = placeService;
            _navigationService = navigationService;
            _prayerTimesConfigurationStorage = prayerTimesConfigurationStorage;
            _logger = logger;
        }

        #region fields

        private readonly IPrayerTimeCalculationService _prayerTimeCalculationService;
        private readonly IPlaceService _placeService;
        private readonly INavigationService _navigationService;
        private readonly PrayerTimesConfigurationStorage _prayerTimesConfigurationStorage;
        private readonly ILogger<MainPageViewModel> _logger;

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
        public List<LocationIQPlace> SearchResults { get; set; }
        [OnChangedMethod(nameof(OnSelectedPlaceChanged))]
        public LocationIQPlace SelectedPlace { get; set; }

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

        public ICommand PerformSearch => new Command<string>(async (string query) =>
        {
            try
            {
                string languageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

                List<LocationIQPlace> places = await _placeService.SearchPlacesAsync(query, languageCode);
                SearchResults = places;
            }
            catch (Exception ex) 
            {
                await doToast(ex.Message);
            }
        });

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
            try
            {
                showHideSpecificTimes();
                await loadPrayerTimes();
            }
            catch (Exception exception)
            {
                await doToast(exception.Message);
            }
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

        public void OnSelectedPlaceChanged()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (SelectedPlace == null)
                    {
                        PrayerTimesConfigurationStorage.MuwaqqitLocationInfo = null;
                        PrayerTimesConfigurationStorage.FaziletLocationInfo = null;
                        PrayerTimesConfigurationStorage.SemerkandLocationInfo = null;
                        return;
                    }

                    _logger.LogDebug("OnSelectedPlaceChanged logic begin");

                    PrayerTimesConfigurationStorage.MuwaqqitLocationInfo =
                        (await _prayerTimeCalculationService
                        .GetPrayerTimeCalculatorByCalculationSource(ECalculationSource.Muwaqqit)
                        .GetLocationInfo(SelectedPlace)) as MuwaqqitLocationInfo;
                    PrayerTimesConfigurationStorage.FaziletLocationInfo =
                        (await _prayerTimeCalculationService
                        .GetPrayerTimeCalculatorByCalculationSource(ECalculationSource.Fazilet)
                        .GetLocationInfo(SelectedPlace)) as FaziletLocationInfo;
                    PrayerTimesConfigurationStorage.SemerkandLocationInfo =
                        (await _prayerTimeCalculationService
                        .GetPrayerTimeCalculatorByCalculationSource(ECalculationSource.Semerkand)
                        .GetLocationInfo(SelectedPlace)) as SemerkandLocationInfo;

                    await this.loadPrayerTimes();
                }
                catch (Exception exception)
                {
                    await doToast(exception.Message);
                }
            });
        }

        public async Task doToast(string text)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            ToastDuration duration = ToastDuration.Short;
            double fontSize = 14;

            var toast = Toast.Make(text, duration, fontSize);

            await toast.Show(cancellationTokenSource.Token);
        }

        #endregion private methods
    }
}
