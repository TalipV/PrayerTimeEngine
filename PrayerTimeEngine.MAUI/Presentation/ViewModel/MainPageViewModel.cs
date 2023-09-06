using System.Windows.Input;
using PropertyChanged;
using PrayerTimeEngine.Presentation.Service.Navigation;
using System.Globalization;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Extensions.Logging;
using MetroLog.Maui;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Core.Domain.PlacesService.Interfaces;
using NodaTime;
using NodaTime.Extensions;
using PrayerTimeEngine.Core.Domain.PlacesService.Models.Common;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain;

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
            ILogger<MainPageViewModel> logger,
            TimeTypeAttributeService timeTypeAttributeService)
        {
            _prayerTimeCalculationService = prayerTimeCalculator;
            _placeService = placeService;
            _configStoreService = configStoreService;
            _navigationService = navigationService;
            _prayerTimesConfigurationStorage = prayerTimesConfigurationStorage;
            _logger = logger;
            _timeTypeAttributeService = timeTypeAttributeService;
        }

        #region fields

        private readonly IPrayerTimeCalculationService _prayerTimeCalculationService;
        private readonly ILocationService _placeService;
        private readonly TimeTypeAttributeService _timeTypeAttributeService;
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

                Instant currentInstant = SystemClock.Instance.GetCurrentInstant();

                return Prayers.AllPrayerTimes.FirstOrDefault(x => x.Start.Value.ToInstant() <= currentInstant && currentInstant <= x.End.Value.ToInstant())
                    ?? Prayers.AllPrayerTimes.OrderBy(x => x.Start.Value.Second).FirstOrDefault(x => x.Start.Value.ToInstant() > currentInstant);
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

        [OnChangedMethod(nameof(onSelectedPlaceChanged))]
        public BasicPlaceInfo SelectedPlace { get; set; }

        public ZonedDateTime? LastUpdated { get; private set; }

        public bool IsLoadingPrayerTimesOrSelectedPlace => IsLoadingPrayerTimes || IsLoadingSelectedPlace;
        public bool IsNotLoadingPrayerTimesOrSelectedPlace => !IsLoadingPrayerTimesOrSelectedPlace;

        public bool IsLoadingSelectedPlace { get; set; }
        public bool IsLoadingPrayerTimes { get; set; }

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

        public ICommand PerformSearch => new Command<string>(async query => await PerformPlaceSearch(query));

        public async Task<List<BasicPlaceInfo>> PerformPlaceSearch(string searchText)
        {
            try
            {
                string languageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                return await _placeService.SearchPlacesAsync(searchText, languageCode);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error during place search");
                doToast(ex.Message);
            }

            return new List<BasicPlaceInfo>();
        }

        public ICommand GoToSettingsPageCommand
            => new Command<EPrayerType>(
                async (prayerTime) =>
                {
                    if (!IsLoadingPrayerTimes)
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
                IsLoadingPrayerTimes = true;

                LocalDate today = DateTime.Now.ToLocalDateTime().Date;
                Prayers = await _prayerTimeCalculationService.ExecuteAsync(CurrentProfile, today);
                OnAfterLoadingPrayerTimes_EventTrigger.Invoke();
            }
            finally
            {
                IsLoadingPrayerTimes = false;
                Interlocked.Exchange(ref isLoadPrayerTimesRunningInterlockedInt, 0);  // Reset the flag to allow future runs
                LastUpdated = SystemClock.Instance.GetCurrentInstant().InZone(DateTimeZoneProviders.Tzdb[TimeZoneInfo.Local.Id]);
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
                if (CurrentProfile == null || SelectedPlace == null)
                {
                    return;
                }

                try
                {
                    this.IsLoadingSelectedPlace = true;
                    CurrentProfile.LocationDataByCalculationSource.Clear();

                    CompletePlaceInfo completePlaceInfo = await _placeService.GetTimezoneInfo(SelectedPlace);

                    foreach (var calculationSource in
                        Enum.GetValues(typeof(ECalculationSource))
                        .Cast<ECalculationSource>())
                    {
                        if (calculationSource == ECalculationSource.None)
                            continue;

                        CurrentProfile.LocationDataByCalculationSource[calculationSource] =
                            await _prayerTimeCalculationService
                                .GetPrayerTimeCalculatorByCalculationSource(calculationSource)
                                .GetLocationInfo(completePlaceInfo);
                    }

                    CurrentProfile.LocationName = completePlaceInfo.DisplayText;
                    await _configStoreService.SaveProfile(CurrentProfile);

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
                finally
                {
                    this.IsLoadingSelectedPlace = false;
                }

                await loadPrayerTimes();
            });
        }

        public string GetLocationDataDisplayText()
        {
            if (this.CurrentProfile == null)
                return "";

            MuwaqqitLocationData muwaqqitLocationData = this.CurrentProfile.LocationDataByCalculationSource[ECalculationSource.Muwaqqit] as MuwaqqitLocationData;
            FaziletLocationData faziletLocationData = this.CurrentProfile.LocationDataByCalculationSource[ECalculationSource.Fazilet] as FaziletLocationData;
            SemerkandLocationData semerkandLocationData = this.CurrentProfile.LocationDataByCalculationSource[ECalculationSource.Semerkand] as SemerkandLocationData;

            return $"""
                    Muwaqqit:
                        - Coordinates:  
                        ({muwaqqitLocationData.Latitude} / {muwaqqitLocationData.Longitude})
                        - Timezone:     
                        '{muwaqqitLocationData.TimezoneName}'
                    
                    Fazilet:
                        - Country 
                        '{faziletLocationData.CountryName}'
                        - City 
                        '{faziletLocationData.CityName}'
                    
                    Semerkand:
                        - Country 
                        '{semerkandLocationData.CountryName}'
                        - City 
                        '{semerkandLocationData.CityName}'
                    """;
        }

        public string GetPrayerTimeConfigDisplayText()
        {
            string outputText = "";

            foreach (KeyValuePair<EPrayerType, List<ETimeType>> item in _timeTypeAttributeService.PrayerTypeToTimeTypes)
            {
                EPrayerType prayerType = item.Key;
                outputText += prayerType.ToString();

                foreach (ETimeType timeType in item.Value)
                {
                    if (!_timeTypeAttributeService.ConfigurableTypes.Contains(timeType))
                        continue;

                    GenericSettingConfiguration config = this.CurrentProfile.Configurations[timeType];

                    outputText += Environment.NewLine;
                    outputText += $"- {timeType} mit {config.Source}";
                    if (config is MuwaqqitDegreeCalculationConfiguration degreeConfig)
                    {
                        outputText += $" ({degreeConfig.Degree}°)";
                    }
                }

                outputText += Environment.NewLine;
                outputText += Environment.NewLine;
            }

            return outputText;
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
