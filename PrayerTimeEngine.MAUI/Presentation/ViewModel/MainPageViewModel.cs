using System.Windows.Input;
using PropertyChanged;
using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Model;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Presentation.Service.Navigation;
using PrayerTimeEngine.Domain.NominatimLocation.Interfaces;
using PrayerTimeEngine.Domain.LocationService.Models;
using System.Globalization;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Extensions.Logging;
using MetroLog.Maui;
using PrayerTimeEngine.Domain.ConfigStore.Interfaces;

namespace PrayerTimeEngine.Presentation.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class MainPageViewModel : LogController
    {
        public MainPageViewModel(
            IPrayerTimeCalculationService prayerTimeCalculator,
            ILocationService placeService,
            IConfigStoreService configStoreService,
            INavigationService navigationService,
            PrayerTimesConfigurationStorage prayerTimesConfigurationStorage,
            ILogger<MainPageViewModel> logger)
        {
            _prayerTimeCalculationService = prayerTimeCalculator;
            _placeService = placeService;
            _configStoreService = configStoreService;
            _navigationService = navigationService;
            _prayerTimesConfigurationStorage = prayerTimesConfigurationStorage;
            _logger = logger;
        }

        #region fields

        private readonly IPrayerTimeCalculationService _prayerTimeCalculationService;
        private readonly ILocationService _placeService;
        private readonly IConfigStoreService _configStoreService;
        private readonly INavigationService _navigationService;
        private readonly PrayerTimesConfigurationStorage _prayerTimesConfigurationStorage;
        private readonly ILogger<MainPageViewModel> _logger;

        #endregion fields

        #region properties

        public PrayerTime DisplayPrayerTime
        {
            get
            {
                // only show data when no information is lacking
                if (Prayers == null || Prayers.AllPrayerTimes.Any(x => x.Start == null || x.End == null))
                {
                    return null;
                }

                DateTime dateTime = DateTime.Now;

                return Prayers.AllPrayerTimes.FirstOrDefault(x => x.Start <= dateTime && dateTime <= x.End)
                    ?? Prayers.AllPrayerTimes.OrderBy(x => x.Start).FirstOrDefault(x => x.Start > dateTime);
            }
        }

        public Profile CurrentProfile
        {
            get
            {
                return Profiles?.FirstOrDefault();
            }
        }

        public List<Profile> Profiles { get; private set; }

        public PrayerTimesBundle Prayers { get; private set; }

        public List<LocationIQPlace> SearchResults { get; set; }

        [OnChangedMethod(nameof(onSelectedPlaceChanged))]
        public LocationIQPlace SelectedPlace { get; set; }

        public DateTime? LastUpdated { get; private set; }
        public bool IsCurrentlyLoadingTimes => this.isLoadPrayerTimesRunningInterlockedInt != 0;
        public bool IsNotLoading => !IsCurrentlyLoadingTimes;

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
            if (this.IsCurrentlyLoadingTimes)
            {
                return;
            }

            try
            {
                string languageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

                List<LocationIQPlace> places = await _placeService.SearchPlacesAsync(query, languageCode);
                SearchResults = places;
            }
            catch (Exception ex) 
            {
                _logger.LogDebug(ex, "Error during place search");
                doToast(ex.Message);
            }
        });

        public ICommand GoToSettingsPageCommand
            => new Command<EPrayerType>(
                async (prayerTime) =>
                {
                    if (!IsCurrentlyLoadingTimes)
                    {
                        await _navigationService.NavigateTo<SettingsHandlerPageViewModel>(prayerTime);
                    }
                });

        public event Action OnAfterLoadingPrayerTimes_EventTrigger = delegate { };

        #endregion ICommand

        #region public methods

        public async void OnActualAppearing()
        {
            try
            {
                if (CurrentProfile == null)
                {
                    return;
                }

                showHideSpecificTimes();
                await loadPrayerTimes();
            }
            catch (Exception exception)
            {
                _logger.LogDebug(exception, "Error during OnActualAppearing");
                doToast(exception.Message);
            }
        }

        public async Task OnPageLoaded()
        {
            try
            {

                Profiles = await _prayerTimesConfigurationStorage.GetProfiles();

                await loadPrayerTimes();
                showHideSpecificTimes();
            }
            catch (Exception exception)
            {
                _logger.LogDebug(exception, "Error during page load");
                doToast(exception.Message);
            }
        }

        #endregion public methods

        #region private methods

        private void createNewProfile()
        {
            // create new profile with SequenceNo = profiles.Select(x => x.SequenceNo).Max();
            // switch to this new profile
        }

        private void deleteCurrentProfile()
        {
            // don't allow if it is the only profile

            // delete current profile and recalculate SequenceNos
            // switch to profil before it or alternatively after it
        }

        private void switchProfile()
        {
            // set CurrentProfile to new value

            // reload prayer times for new profile
        }

        private int isLoadPrayerTimesRunningInterlockedInt = 0;  // 0 for false, 1 for true

        private async Task loadPrayerTimes()
        {
            if (CurrentProfile == null || Interlocked.CompareExchange(ref isLoadPrayerTimesRunningInterlockedInt, 1, 0) == 1)
            {
                return;
            }

            try
            {
                Prayers = await _prayerTimeCalculationService.ExecuteAsync(CurrentProfile, DateTime.Today);
                OnAfterLoadingPrayerTimes_EventTrigger.Invoke();
            }
            finally
            {
                Interlocked.Exchange(ref isLoadPrayerTimesRunningInterlockedInt, 0);  // Reset the flag to allow future runs
                LastUpdated = DateTime.Now;
            }
        }

        private void showHideSpecificTimes()
        {
            ShowFajrGhalas = isCalculationShown(ETimeType.FajrGhalas);
            ShowFajrRedness = isCalculationShown(ETimeType.FajrKaraha);
            ShowDuhaQuarter = isCalculationShown(ETimeType.DuhaQuarterOfDay);
            ShowMithlayn = isCalculationShown(ETimeType.AsrMithlayn);
            ShowKaraha = isCalculationShown(ETimeType.AsrKaraha);
            ShowMaghribSufficientTime = isCalculationShown(ETimeType.MaghribSufficientTime);
            ShowIshtibaq = isCalculationShown(ETimeType.MaghribIshtibaq);
            ShowOneThird = isCalculationShown(ETimeType.IshaFirstThird);
            ShowTwoThird = isCalculationShown(ETimeType.IshaSecondThird);
            ShowMidnight = isCalculationShown(ETimeType.IshaMidnight);

            OnPropertyChanged();
        }

        private bool isCalculationShown(ETimeType timeData)
        {
            if (!CurrentProfile.Configurations.TryGetValue(timeData, out GenericSettingConfiguration config) || config == null)
            {
                return true;
            }

            return config.IsTimeShown;
        }

        private void onSelectedPlaceChanged()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (CurrentProfile == null || SelectedPlace == null)
                    {
                        return;
                    }

                    this.SearchResults.Clear();
                    CurrentProfile.LocationDataByCalculationSource.Clear();

                    foreach (var calculationSource in
                        Enum.GetValues(typeof(ECalculationSource))
                        .Cast<ECalculationSource>())
                    {
                        if (calculationSource == ECalculationSource.None)
                            continue;

                        CurrentProfile.LocationDataByCalculationSource[calculationSource] =
                            await _prayerTimeCalculationService
                                .GetPrayerTimeCalculatorByCalculationSource(calculationSource)
                                .GetLocationInfo(SelectedPlace);
                    }

                    CurrentProfile.LocationName = SelectedPlace.address.city;
                    await _configStoreService.SaveProfile(CurrentProfile);

                    await loadPrayerTimes();

                    var missingLocationInfo =
                        CurrentProfile.LocationDataByCalculationSource
                            .Where(x => x.Value == null)
                            .Select(x => x.Key.ToString())
                            .ToList();

                    if (missingLocationInfo.Count != 0)
                    {
                        doToast($"Location information missing for {string.Join(", ", missingLocationInfo)}");
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogDebug(exception, "Error during place selection");
                    doToast(exception.Message);
                }
            });
        }

        // TODO REFACTOR
        private static void doToast(string text)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                ToastDuration duration = ToastDuration.Short;
                double fontSize = 14;

                var toast = Toast.Make(text, duration, fontSize);

                await toast.Show(cancellationTokenSource.Token);
            });
        }

        #endregion private methods
    }
}
