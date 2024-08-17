using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using MetroLog.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.CalculationManagement;
using PrayerTimeEngine.Core.Domain.Calculators;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Service;
using PrayerTimeEngine.Presentation.Service.Navigation;
using PrayerTimeEngine.Services.PrayerTimeSummaryNotification;
using PropertyChanged;
using System;
using System.Windows.Input;

namespace PrayerTimeEngine.Presentation.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class MainPageViewModel(
            IDispatcher dispatcher,
            ToastMessageService toastMessageService,
            ISystemInfoService _systemInfoService,
            ICalculationManager prayerTimeCalculator,
            IPrayerTimeCalculatorFactory prayerTimeServiceFactory,
            IPlaceService placeService,
            IProfileService profileService,
            INavigationService navigationService,
            ILogger<MainPageViewModel> logger
        ) : LogController
    {
        #region fields

        private Debounce.Core.Debouncer debouncer;
        private bool _isSettingsPageOpening = false;

        #endregion fields

        #region properties

        public Profile CurrentProfile { get; set; }
        public PrayerTimesBundle PrayerTimeBundle { get; private set; }

        [OnChangedMethod(nameof(onSelectedPlaceChanged))]
        public string SelectedPlaceText { get; set; }

        public bool IsLoadingPrayerTimes { get; set; }

        public bool IsLoadingPrayerTimesOrSelectedPlace => IsLoadingPrayerTimes || IsLoadingSelectedPlace;
        public bool IsNotLoadingPrayerTimesOrSelectedPlace => !IsLoadingPrayerTimesOrSelectedPlace;

        public bool IsLoadingSelectedPlace { get; set; }
        public bool IsNotLoadingSelectedPlace => !IsLoadingSelectedPlace;

        public bool ShowFajrGhalas { get; set; }
        public bool ShowFajrRedness { get; set; }
        public bool ShowMithlayn { get; set; }
        public bool ShowKaraha { get; set; }
        public bool ShowIshtibaq { get; set; }
        public bool ShowMaghribSufficientTime { get; set; }

        [OnChangedMethod(nameof(onPlaceSearchTextChanged))]
        public string PlaceSearchText { get; set; }
        public IEnumerable<BasicPlaceInfo> FoundPlaces { get; set; }
        public IEnumerable<string> FoundPlacesSelectionTexts { get; set; }

        #endregion properties

        #region ICommand

        public ICommand GoToSettingsPageCommand
            => new Command<EPrayerType>(
                async (prayerTime) =>
                {
                    if (_isSettingsPageOpening)
                    {
                        return;
                    }

                    // stop loading times and stop loading places
                    loadingTimesCancellationTokenSource?.Cancel();
                    _placeSearchCancellationTokenSource?.Cancel();

                    try
                    {
                        _isSettingsPageOpening = true;
                        await navigationService.NavigateTo<SettingsHandlerPageViewModel>(CurrentProfile, prayerTime);
                    }
                    finally
                    {
                        _isSettingsPageOpening = false;
                    }
                });

        public event Action OnAfterLoadingPrayerTimes_EventTrigger = delegate { };

        #endregion ICommand

        #region public methods

        public void onPlaceSearchTextChanged()
        {
            if (!isValidPlaceSearchText())
            {
                this.resetPlaceInput(resetPlaceSearchText: false);
                return;
            }

            debouncer ??= new Debounce.Core.Debouncer(searchPlace, intervalMs: 600);
            debouncer.Debounce();
        }

        private async void searchPlace()
        {
            if (!isValidPlaceSearchText())
            {
                return;
            }
            FoundPlaces = await PerformPlaceSearch(this.PlaceSearchText);
            FoundPlacesSelectionTexts = FoundPlaces.Select(x => x.DisplayText).Distinct().OrderBy(x => x).Take(7).ToList();
        }

        private bool isValidPlaceSearchText()
        {
            return !string.IsNullOrWhiteSpace(this.PlaceSearchText) 
                && this.PlaceSearchText.Replace(" ", string.Empty).Length > 4;
        }

        private CancellationTokenSource _placeSearchCancellationTokenSource;

        public async Task<List<BasicPlaceInfo>> PerformPlaceSearch(string searchText)
        {
            logger.LogInformation("Place search triggered with search text '{SearchText}'", searchText);

            var currentTokenSource = new CancellationTokenSource();
            try
            {
                // cancel a previous request, if there is one
                _placeSearchCancellationTokenSource?.Cancel();
                _placeSearchCancellationTokenSource = currentTokenSource;

                string languageCode = _systemInfoService.GetSystemCulture().TwoLetterISOLanguageName;
                return await placeService.SearchPlacesAsync(searchText, languageCode, currentTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Place search was canceled.");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error during place search");
                toastMessageService.ShowError(exception.Message);
            }
            finally
            {
                currentTokenSource?.Dispose();

                if (_placeSearchCancellationTokenSource == currentTokenSource)
                    _placeSearchCancellationTokenSource = null;
            }

            return [];
        }

        public async Task OnActualAppearing()
        {
            await refreshData();
        }

        #endregion public methods

        #region private methods

#pragma warning disable CA1822 // Mark members as static
        private void createNewProfile()
        {
            // create new profile with SequenceNo = profiles.Select(x => x.SequenceNo).Max();

            // switch to this new profile
        }
        private void deleteCurrentProfile()
        {
            // don't allow if it is the only profile

            // delete current profile and recalculate SequenceNos

            // switch to profile before it or alternatively after it
        }
        private void switchProfile()
        {
            // set CurrentProfile to new value

            // reload prayer times for new profile
        }
#pragma warning restore CA1822 // Mark members as static

        // concurrent loading is prevent in that subsequent loading requests are ignored when a previous one is currently running
        // but that one can be cancelled by things like leaving the page
        private long isLoadPrayerTimesRunningInterlockedInt = 0;  // 0 for false, 1 for true
        private CancellationTokenSource loadingTimesCancellationTokenSource;

        private async Task refreshData()
        {
            string refreshCallID = DebugUtil.GenerateDebugID();
            logger.LogInformation("Refreshing data started. ({RefreshCallID}) {StackTrace}", refreshCallID, new System.Diagnostics.StackTrace().ToString());

            try
            {
                if (!MauiProgram.IsFullyInitialized)
                    await onBeforeFirstLoad();

                try
                {
                    if (CurrentProfile is null || Interlocked.CompareExchange(ref isLoadPrayerTimesRunningInterlockedInt, 1, 0) == 1)
                    {
                        return;
                    }

                    this.IsLoadingPrayerTimes = true;

                    await showHideSpecificTimes();

                    loadingTimesCancellationTokenSource?.Cancel();
                    loadingTimesCancellationTokenSource?.Dispose();
                    loadingTimesCancellationTokenSource = new CancellationTokenSource();

                    ZonedDateTime zonedDateTime = 
                        _systemInfoService.GetCurrentInstant()
                            .InZone(DateTimeZoneProviders.Tzdb[CurrentProfile.PlaceInfo.TimezoneInfo.Name]);

                    PrayerTimeBundle = 
                        await prayerTimeCalculator.CalculatePrayerTimesAsync(
                            CurrentProfile.ID, 
                            zonedDateTime, 
                            loadingTimesCancellationTokenSource.Token);

                    OnAfterLoadingPrayerTimes_EventTrigger?.Invoke();
                }
                finally
                {
                    Interlocked.Exchange(ref isLoadPrayerTimesRunningInterlockedInt, 0);  // Reset the flag to allow future runs
                    this.IsLoadingPrayerTimes = false;
                }

                if (!MauiProgram.IsFullyInitialized)
                {
                    onAfterFirstLoad();
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Loading times was canceled. ({RefreshCallID})", refreshCallID);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error during refreshData. ({RefreshCallID})", refreshCallID);
                toastMessageService.ShowError(exception.Message);
            }
            finally
            {
                MauiProgram.IsFullyInitialized = true;
            }

            logger.LogInformation("Refreshing data finished. ({RefreshCallID})", refreshCallID);
        }

        private async Task onBeforeFirstLoad()
        {
            // TODO put this somewhere else
            await MauiProgram.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
            CurrentProfile ??= (await profileService.GetProfiles(cancellationToken: default)).First();
        }        
        
        private void onAfterFirstLoad()
        {
            try
            {
                double startUpTimeMS = (DateTime.Now - MauiProgram.StartDateTime).TotalMilliseconds;
                toastMessageService.Show($"{startUpTimeMS:N0}ms to start!");
#if ANDROID
                dispatcher.Dispatch(
                    async () =>
                    {
                        var notificationManager = MauiProgram.ServiceProvider.GetRequiredService<PrayerTimeSummaryNotificationManager>();
                        await notificationManager.TryStartPersistentNotification();
                    });
#endif
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error during onAfterFirstLoad");
                toastMessageService.ShowError(exception?.Message);
            }
        }

        private async Task showHideSpecificTimes()
        {
            await dispatcher.DispatchAsync(() =>
            {
                if (CurrentProfile is null)
                    return;

                ShowFajrGhalas = isCalculationShown(ETimeType.FajrGhalas);
                ShowFajrRedness = isCalculationShown(ETimeType.FajrKaraha);

                ShowMithlayn = isCalculationShown(ETimeType.AsrMithlayn);
                ShowKaraha = isCalculationShown(ETimeType.AsrKaraha);

                ShowMaghribSufficientTime = isCalculationShown(ETimeType.MaghribSufficientTime);
                ShowIshtibaq = isCalculationShown(ETimeType.MaghribIshtibaq);
            });
        }

        private bool isCalculationShown(ETimeType timeData)
        {
            return profileService.GetTimeConfig(CurrentProfile, timeData)?.IsTimeShown == true;
        }

        private void onSelectedPlaceChanged()
        {
            string selectedPlaceText = this.SelectedPlaceText;

            if (string.IsNullOrWhiteSpace(selectedPlaceText))
            {
                return;
            }

            logger.LogInformation("Place '{SelectedPlace}' was selected", selectedPlaceText);

            Task.Run(async () =>
            {
                BasicPlaceInfo SelectedPlace = this.FoundPlaces.FirstOrDefault(x => x.DisplayText == selectedPlaceText);

                if (CurrentProfile is null || SelectedPlace is null)
                    return;

                await dispatcher.DispatchAsync(() => resetPlaceInput());

                try
                {
                    this.IsLoadingSelectedPlace = true;

                    CompletePlaceInfo completePlaceInfo = await placeService.GetTimezoneInfo(SelectedPlace, cancellationToken: default);
                    var locationDataWithCalculationSource = await getCalculationSourceWithLocationData(completePlaceInfo, cancellationToken: default);

                    await profileService.UpdateLocationConfig(
                        CurrentProfile,
                        completePlaceInfo, 
                        locationDataWithCalculationSource,
                    cancellationToken: default);

                    // notify UI for updates
                    await dispatcher.DispatchAsync(() => OnPropertyChanged(nameof(CurrentProfile)));

                    HashSet<ECalculationSource> locationConfigCalculationSources = 
                        this.CurrentProfile.LocationConfigs
                            .Select(x => x.CalculationSource)
                            .ToHashSet();
                    List<ECalculationSource> missingLocationInfo =
                        Enum.GetValues<ECalculationSource>()
                            .Where(enumValue => enumValue != ECalculationSource.None && !locationConfigCalculationSources.Contains(enumValue))
                            .ToList();
                    if (missingLocationInfo.Count != 0)
                    {
                        logger.LogWarning("Location information missing for the following calculation sources: {CalculationSources}", string.Join(", ", missingLocationInfo));
                        toastMessageService.ShowWarning($"Location information missing for {string.Join(", ", missingLocationInfo)}");
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Error during place selection");
                    toastMessageService.ShowError(exception.Message);
                }
                finally
                {
                    this.IsLoadingSelectedPlace = false;
                }

                await refreshData();
            });
        }

        private void resetPlaceInput(bool resetPlaceSearchText = true)
        {
            if (resetPlaceSearchText)
                this.PlaceSearchText = string.Empty;

            this.SelectedPlaceText = string.Empty;
            this.FoundPlaces = [];
            this.FoundPlacesSelectionTexts = [];
        }

        private async Task<List<(ECalculationSource, BaseLocationData)>> getCalculationSourceWithLocationData(CompletePlaceInfo completePlaceInfo, CancellationToken cancellationToken)
        {
            var locationDataWithCalculationSource = new List<(ECalculationSource, BaseLocationData)>();

            foreach (var calculationSource in Enum.GetValues<ECalculationSource>())
            {
                if (calculationSource == ECalculationSource.None)
                    continue;

                BaseLocationData locationConfig =
                    await prayerTimeServiceFactory
                        .GetPrayerTimeCalculatorByCalculationSource(calculationSource)
                        .GetLocationInfo(completePlaceInfo, cancellationToken);

                locationDataWithCalculationSource.Add((calculationSource, locationConfig));
            }

            return locationDataWithCalculationSource;
        }

        public PrayerTime GetDisplayPrayerTime()
        {
            // only show data when no information is lacking
            if (PrayerTimeBundle is null || PrayerTimeBundle.AllPrayerTimes.Any(x => x.Start is null || x.End is null))
            {
                return null;
            }

            Instant currentInstant = _systemInfoService.GetCurrentInstant();

            return PrayerTimeBundle.AllPrayerTimes.FirstOrDefault(x => x.Start.Value.ToInstant() <= currentInstant && currentInstant <= x.End.Value.ToInstant())
                ?? PrayerTimeBundle.AllPrayerTimes.OrderBy(x => x.Start.Value.ToInstant()).FirstOrDefault(x => x.Start.Value.ToInstant() > currentInstant);
        }

        public async Task ShowDatabaseTable()
        {
            await navigationService.NavigateTo<DatabaseTablesPageViewModel>();
        }

        public string GetPrayerTimeConfigDisplayText()
        {
            return profileService.GetPrayerTimeConfigDisplayText(CurrentProfile);
        }

        internal string GetLocationDataDisplayText()
        {
            return profileService.GetLocationDataDisplayText(CurrentProfile);
        }

        #endregion private methods
    }
}
