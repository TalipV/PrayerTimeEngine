using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using MethodTimer;
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
            PrayerTimeSummaryNotificationManager prayerTimeSummaryNotificationManager,
            ILogger<MainPageViewModel> logger
        ) : LogController
    {
        #region properties

        public Profile CurrentProfile { get; set; }
        public PrayerTimesBundle PrayerTimeBundle { get; private set; }

        [OnChangedMethod(nameof(onSelectedPlaceChanged))]
        public BasicPlaceInfo SelectedPlace { get; set; }


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

        #endregion properties

        #region ICommand

        public ICommand PerformSearch => new Command<string>(async query => await PerformPlaceSearch(query));

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

        public async void OnActualAppearing()
        {
            try
            {
                if (CurrentProfile == null)
                    return;

                showHideSpecificTimes();
                await loadPrayerTimes();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error during OnActualAppearing");
                showToastMessage(exception.Message);
            }
        }

        // TODO
        // - PrayerTimeCalculationService bei jedem Laden den Cache aufsetzen lassen
        // - Cache Invalidierung anhand von Equals/HashCode checks
        // - Notification genauso Zeiten laden lassen

        [Time]
        public async Task OnPageLoaded()
        {
            try
            {
                if (!MauiProgram.IsFullyInitialized)
                    await appDbContext.Database.MigrateAsync();

                logger.LogInformation("OnPageLoaded-Start");
                CurrentProfile ??= (await profileService.GetProfiles().ConfigureAwait(false)).First();

                await loadPrayerTimes();
                showHideSpecificTimes();

                if (!MauiProgram.IsFullyInitialized)
                {
                    double startUpTimeMS = (DateTime.Now - MauiProgram.StartDateTime).TotalMilliseconds;
                    showToastMessage($"{startUpTimeMS:N0}ms to start!");

                    prayerTimeSummaryNotificationManager.TryStartPersistentNotification();
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error during page load");
                showToastMessage(exception.Message);
            }
            finally
            {
                MauiProgram.IsFullyInitialized = true;
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
                PrayerTimeBundle = await prayerTimeCalculator.CalculatePrayerTimesAsync(CurrentProfile, _systemInfoService.GetCurrentZonedDateTime());

                OnAfterLoadingPrayerTimes_EventTrigger.Invoke();
            }
            finally
            {
                IsLoadingPrayerTimes = false;
                Interlocked.Exchange(ref isLoadPrayerTimesRunningInterlockedInt, 0);  // Reset the flag to allow future runs
            }
        }

        private void showHideSpecificTimes()
        {
            if (CurrentProfile == null)
                return;

            ShowFajrGhalas = isCalculationShown(ETimeType.FajrGhalas);
            ShowFajrRedness = isCalculationShown(ETimeType.FajrKaraha);

            ShowMithlayn = isCalculationShown(ETimeType.AsrMithlayn);
            ShowKaraha = isCalculationShown(ETimeType.AsrKaraha);

            ShowMaghribSufficientTime = isCalculationShown(ETimeType.MaghribSufficientTime);
            ShowIshtibaq = isCalculationShown(ETimeType.MaghribIshtibaq);
        }

        private bool isCalculationShown(ETimeType timeData)
        {
            return profileService.GetTimeConfig(CurrentProfile, timeData)?.IsTimeShown == true;
        }

        private void onSelectedPlaceChanged()
        {
            Task.Run(async () =>
            {
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

                await loadPrayerTimes();
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
