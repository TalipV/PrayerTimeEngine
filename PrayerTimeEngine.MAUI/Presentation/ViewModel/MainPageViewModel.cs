using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using MetroLog.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.CalculationManagement;
using PrayerTimeEngine.Core.Domain.Calculators;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models.Common;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;
using PrayerTimeEngine.Presentation.Service.Navigation;
using PrayerTimeEngine.Services;
using PrayerTimeEngine.Services.SystemInfoService;
using PropertyChanged;
using System.Windows.Input;

namespace PrayerTimeEngine.Presentation.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class MainPageViewModel(
            ISystemInfoService _systemInfoService,
            ICalculationManager prayerTimeCalculator,
            IPrayerTimeServiceFactory prayerTimeServiceFactory,
            IPlaceService placeService,
            IProfileService profileService,
            INavigationService navigationService,
            AppDbContext appDbContext,
            ILogger<MainPageViewModel> logger
        ) : LogController
    {
        #region fields

        private Debounce.Core.Debouncer debouncer;

        #endregion fields

        #region properties

        public Profile CurrentProfile { get; set; }
        public PrayerTimesBundle PrayerTimeBundle { get; private set; }

        [OnChangedMethod(nameof(onSelectedPlaceChanged))]
        public string SelectedPlaceText { get; set; }


        public bool IsLoadingPrayerTimesOrSelectedPlace => IsLoadingPrayerTimes || IsLoadingSelectedPlace;
        public bool IsNotLoadingPrayerTimesOrSelectedPlace => !IsLoadingPrayerTimesOrSelectedPlace;

        public bool IsLoadingSelectedPlace { get; set; }
        public bool IsLoadingPrayerTimes { get; set; }

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
                    if (!IsLoadingPrayerTimes)
                    {
                        await navigationService.NavigateTo<SettingsHandlerPageViewModel>(CurrentProfile, prayerTime);
                    }
                });

        public event Action OnAfterLoadingPrayerTimes_EventTrigger = delegate { };

        #endregion ICommand

        #region public methods

        public void onPlaceSearchTextChanged()
        {
            if (string.IsNullOrWhiteSpace(this.PlaceSearchText))
            {
                return;
            }

            debouncer ??= new Debounce.Core.Debouncer(async () =>
            {
                FoundPlaces = await Task.Run(async () => await PerformPlaceSearch(PlaceSearchText));
                FoundPlacesSelectionTexts = FoundPlaces.Select(x => x.DisplayText).Distinct().OrderBy(x => x).Take(7).ToList();
            }, 500);

            debouncer.Debounce();
        }

        public async Task<List<BasicPlaceInfo>> PerformPlaceSearch(string searchText)
        {
            try
            {
                string languageCode = _systemInfoService.GetSystemCulture().TwoLetterISOLanguageName;
                return await placeService.SearchPlacesAsync(searchText, languageCode);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error during place search");
                showToastMessage(exception.Message);
            }

            return [];
        }

        public async Task OnActualAppearing()
        {
            await refreshData();
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

        private async Task refreshData()
        {
            try
            {
                if (!MauiProgram.IsFullyInitialized)
                    await onBeforeFirstLoad();

                await showHideSpecificTimes();

                if (CurrentProfile == null || Interlocked.CompareExchange(ref isLoadPrayerTimesRunningInterlockedInt, 1, 0) == 1)
                {
                    return;
                }

                try
                {
                    IsLoadingPrayerTimes = true;
                    PrayerTimeBundle = await prayerTimeCalculator.CalculatePrayerTimesAsync(CurrentProfile.ID, _systemInfoService.GetCurrentZonedDateTime());
                    OnAfterLoadingPrayerTimes_EventTrigger.Invoke();
                }
                finally
                {
                    IsLoadingPrayerTimes = false;
                    Interlocked.Exchange(ref isLoadPrayerTimesRunningInterlockedInt, 0);  // Reset the flag to allow future runs
                }

                if (!MauiProgram.IsFullyInitialized)
                {
                    onAfterFirstLoad();
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error during refreshData");
                showToastMessage(exception?.Message);
            }
            finally
            {
                MauiProgram.IsFullyInitialized = true;
            }
        }

        private async Task onBeforeFirstLoad()
        {
            await appDbContext.Database.MigrateAsync();
            CurrentProfile ??= (await profileService.GetProfiles()).First();
        }        
        
        private void onAfterFirstLoad()
        {
            double startUpTimeMS = (DateTime.Now - MauiProgram.StartDateTime).TotalMilliseconds;
            showToastMessage($"{startUpTimeMS:N0}ms to start!");
#if ANDROID
            MauiProgram.ServiceProvider.GetRequiredService<PrayerTimeSummaryNotificationManager>().TryStartPersistentNotification();
#endif
        }

        private async Task showHideSpecificTimes()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (CurrentProfile == null)
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
            Task.Run(async () =>
            {
                BasicPlaceInfo SelectedPlace = this.FoundPlaces.FirstOrDefault(x => x.DisplayText == this.SelectedPlaceText);

                if (CurrentProfile == null || SelectedPlace == null)
                    return;

                try
                {
                    this.IsLoadingSelectedPlace = true;

                    CompletePlaceInfo completePlaceInfo = await placeService.GetTimezoneInfo(SelectedPlace);
                    var locationDataWithCalculationSource = await getCalculationSourceWithLocationData(completePlaceInfo);

                    await profileService.UpdateLocationConfig(CurrentProfile, SelectedPlace.DisplayText, locationDataWithCalculationSource);

                    List<ECalculationSource> missingLocationInfo =
                        Enum.GetValues<ECalculationSource>()
                            .Where(enumValue => !CurrentProfile.LocationConfigs.Select(x => x.CalculationSource).Contains(enumValue))
                            .ToList();

                    if (missingLocationInfo.Count != 0)
                    {
                        showToastMessage($"Location information missing for {string.Join(", ", missingLocationInfo)}");
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Error during place selection");
                    showToastMessage(exception.Message);
                }
                finally
                {
                    this.IsLoadingSelectedPlace = false;
                }

                await refreshData();
            });
        }

        private async Task<List<(ECalculationSource, BaseLocationData)>> getCalculationSourceWithLocationData(CompletePlaceInfo completePlaceInfo)
        {
            var locationDataWithCalculationSource = new List<(ECalculationSource, BaseLocationData)>();

            foreach (var calculationSource in Enum.GetValues<ECalculationSource>())
            {
                if (calculationSource == ECalculationSource.None)
                    continue;

                BaseLocationData locationConfig =
                    await prayerTimeServiceFactory
                        .GetPrayerTimeCalculatorByCalculationSource(calculationSource)
                        .GetLocationInfo(completePlaceInfo);

                locationDataWithCalculationSource.Add((calculationSource, locationConfig));
            }

            return locationDataWithCalculationSource;
        }

        public PrayerTime GetDisplayPrayerTime()
        {
            // only show data when no information is lacking
            if (PrayerTimeBundle == null || PrayerTimeBundle.AllPrayerTimes.Any(x => x.Start == null || x.End == null))
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

        // TODO REFACTOR
        private static void showToastMessage(string text)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Toast.Make(
                        message: text, 
                        duration: ToastDuration.Short, 
                        textSize: 14)
                    .Show(default);
            });
        }

#endregion private methods
    }
}
