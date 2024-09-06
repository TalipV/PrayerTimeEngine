using MetroLog.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Pages.DatabaseTables;
using PrayerTimeEngine.Presentation.Pages.Settings.SettingsHandler;
using PrayerTimeEngine.Presentation.Services;
using PrayerTimeEngine.Presentation.Services.Navigation;
using PrayerTimeEngine.Presentation.Views.MosquePrayerTime;
using PrayerTimeEngine.Presentation.Views.PrayerTimes;
using PrayerTimeEngine.Services.PrayerTimeSummaryNotification;
using PropertyChanged;
using System.Windows.Input;

namespace PrayerTimeEngine.Presentation.Pages.Main
{
    [AddINotifyPropertyChangedInterface]
    public class MainPageViewModel(
            IDispatcher dispatcher,
            ToastMessageService toastMessageService,
            ISystemInfoService _systemInfoService,
            IDynamicPrayerTimeProviderFactory prayerTimeServiceFactory,
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

        public List<IPrayerTimeViewModel> ProfilesWithModel { get; set; }

        [OnChangedMethod(nameof(onCurrentProfileWithModelChanged))]
        public IPrayerTimeViewModel CurrentProfileWithModel { get; set; }
        public Profile CurrentProfile
        {
            get
            {
                return CurrentProfileWithModel?.Profile;
            }
        }
        public string ProfileDisplayText => $"{CurrentProfile?.PlaceInfo?.City ?? "-"}, {CurrentProfile?.SequenceNo ?? -1}";


        [OnChangedMethod(nameof(onSelectedPlaceChanged))]
        public string SelectedPlaceText { get; set; }

        public bool IsLoadingPrayerTimes { get; set; }

        public double LoadingStatusOpacityValue
        {
            get
            {
                if (IsLoadingPrayerTimesOrSelectedPlace)
                {
                    return 0.8;
                }

                return 1;
            }
        }
        public bool IsLoadingPrayerTimesOrSelectedPlace => IsLoadingPrayerTimes || IsLoadingSelectedPlace;
        public bool IsNotLoadingPrayerTimesOrSelectedPlace => !IsLoadingPrayerTimesOrSelectedPlace;

        public bool IsLoadingSelectedPlace { get; set; }
        public bool IsNotLoadingSelectedPlace => !IsLoadingSelectedPlace;

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
                resetPlaceInput(resetPlaceSearchText: false);
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
            FoundPlaces = await PerformPlaceSearch(PlaceSearchText);
            FoundPlacesSelectionTexts = FoundPlaces.Select(x => x.DisplayText).Distinct().OrderBy(x => x).Take(7).ToList();
        }

        private bool isValidPlaceSearchText()
        {
            return !string.IsNullOrWhiteSpace(PlaceSearchText)
                && PlaceSearchText.Replace(" ", string.Empty).Length > 4;
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

        private async Task reloadProfiles(int selectedProfile = 1)
        {
            List<Profile> profiles = await profileService.GetProfiles(cancellationToken: default);

            bool oldValue = _suspendOnCurrentProfileWithModelChanged;
            try
            {
                _suspendOnCurrentProfileWithModelChanged = true;
                ProfilesWithModel = profiles.Select(getPrayerTimeViewModel).ToList();
            }
            finally
            {
                _suspendOnCurrentProfileWithModelChanged = oldValue;
            }
            
            CurrentProfileWithModel =
                ProfilesWithModel.FirstOrDefault(x => x.Profile.ID == selectedProfile)
                ?? CurrentProfileWithModel
                ?? ProfilesWithModel.First();
        }

        public async Task CreateNewProfile()
        {
            CancellationToken cancellationToken = default;

            var copiedProfile = await profileService.CopyProfile(CurrentProfile, cancellationToken);
            await reloadProfiles(copiedProfile.ID);
        }

        public async Task CreateNewMosqueProfile()
        {
            CancellationToken cancellationToken = default;

            var copiedProfile = await profileService.CopyProfile(CurrentProfile, cancellationToken);

            var dbContextFactory = MauiProgram.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(default))
            {
                dbContext.Entry(copiedProfile).State = EntityState.Unchanged;
                copiedProfile.IsMosqueProfile = true;
                await dbContext.SaveChangesAsync();
            }

            await reloadProfiles(copiedProfile.ID);
        }

        public async Task DeleteCurrentProfile()
        {
            CancellationToken cancellationToken = default;

            await profileService.DeleteProfile(CurrentProfile, cancellationToken);
            await reloadProfiles();
        }

        // concurrent loading is prevent in that subsequent loading requests are ignored when a previous one is currently running
        // but that one can be cancelled by things like leaving the page
        private long isLoadPrayerTimesRunningInterlockedInt = 0;  // 0 for false, 1 for true
        private CancellationTokenSource loadingTimesCancellationTokenSource;

        private async Task refreshData()
        {
            string refreshCallID = DebugUtil.GenerateDebugID();
            logger.LogInformation("Refreshing data started. ({RefreshCallID})", refreshCallID);

            try
            {
                if (!MauiProgram.IsFullyInitialized)
                    await onBeforeFirstLoad();

                try
                {
                    var currentProfile = CurrentProfile;

                    if (currentProfile is null || Interlocked.CompareExchange(ref isLoadPrayerTimesRunningInterlockedInt, 1, 0) == 1)
                    {
                        return;
                    }

                    IsLoadingPrayerTimes = true;

                    loadingTimesCancellationTokenSource?.Cancel();
                    loadingTimesCancellationTokenSource?.Dispose();
                    loadingTimesCancellationTokenSource = new CancellationTokenSource();

                    ZonedDateTime zonedDateTime =
                        _systemInfoService.GetCurrentInstant()
                            .InZone(DateTimeZoneProviders.Tzdb[currentProfile.PlaceInfo.TimezoneInfo.Name]);

                    await CurrentProfileWithModel.RefreshData(zonedDateTime, loadingTimesCancellationTokenSource.Token);

                    OnAfterLoadingPrayerTimes_EventTrigger?.Invoke();
                }
                finally
                {
                    Interlocked.Exchange(ref isLoadPrayerTimesRunningInterlockedInt, 0);  // Reset the flag to allow future runs
                    IsLoadingPrayerTimes = false;
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

        private IPrayerTimeViewModel getPrayerTimeViewModel(Profile profile)
        {
            IPrayerTimeViewModel viewModel = profile.IsMosqueProfile
                ? MauiProgram.ServiceProvider.GetRequiredService<MosquePrayerTimeViewModel>()
                : MauiProgram.ServiceProvider.GetRequiredService<DynamicPrayerTimeViewModel>();

            viewModel.MainPageViewModel = this;
            viewModel.Profile = profile;

            return viewModel;
        }

        private async Task onBeforeFirstLoad()
        {
            var dbContextFactory = MauiProgram.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();

            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(default))
            {
                // TODO put this somewhere else
                await dbContext.Database.MigrateAsync();
            }

            try
            {
                _suspendOnCurrentProfileWithModelChanged = true;
                await reloadProfiles();
            }
            finally
            {
                _suspendOnCurrentProfileWithModelChanged = false;
            }
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

        // TODO use different approach
        private bool _suspendOnCurrentProfileWithModelChanged = false;

        private async void onCurrentProfileWithModelChanged()
        {
            if (_suspendOnCurrentProfileWithModelChanged || CurrentProfileWithModel == null)
            {
                return;
            }

            await refreshData();
        }

        private void onSelectedPlaceChanged()
        {
            string selectedPlaceText = SelectedPlaceText;

            if (string.IsNullOrWhiteSpace(selectedPlaceText))
            {
                return;
            }

            logger.LogInformation("Place '{SelectedPlace}' was selected", selectedPlaceText);

            Task.Run(async () =>
            {
                BasicPlaceInfo SelectedPlace = FoundPlaces.FirstOrDefault(x => x.DisplayText == selectedPlaceText);

                Profile currentProfile = CurrentProfile;

                if (currentProfile is null || SelectedPlace is null)
                    return;

                await dispatcher.DispatchAsync(() => resetPlaceInput());

                try
                {
                    IsLoadingSelectedPlace = true;

                    ProfilePlaceInfo completePlaceInfo = await placeService.GetTimezoneInfo(SelectedPlace, cancellationToken: default);
                    var locationDataWithDynamicPrayerTimeProvider = await getDynamicPrayerTimeProviderWithLocationData(completePlaceInfo, cancellationToken: default);

                    await profileService.UpdateLocationConfig(
                        currentProfile,
                        completePlaceInfo,
                        locationDataWithDynamicPrayerTimeProvider,
                    cancellationToken: default);

                    // notify UI for updates
                    await dispatcher.DispatchAsync(() => OnPropertyChanged(nameof(CurrentProfile)));

                    HashSet<EDynamicPrayerTimeProviderType> locationConfigDynamicPrayerTimeProviders =
                        currentProfile.LocationConfigs
                            .Select(x => x.DynamicPrayerTimeProvider)
                            .ToHashSet();
                    List<EDynamicPrayerTimeProviderType> missingLocationInfo =
                        Enum.GetValues<EDynamicPrayerTimeProviderType>()
                            .Where(enumValue => enumValue != EDynamicPrayerTimeProviderType.None && !locationConfigDynamicPrayerTimeProviders.Contains(enumValue))
                            .ToList();
                    if (missingLocationInfo.Count != 0)
                    {
                        logger.LogWarning("Location information missing for the following calculation sources: {DynamicPrayerTimeProviders}", string.Join(", ", missingLocationInfo));
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
                    IsLoadingSelectedPlace = false;
                }

                await refreshData();
            });
        }

        private void resetPlaceInput(bool resetPlaceSearchText = true)
        {
            if (resetPlaceSearchText)
                PlaceSearchText = string.Empty;

            SelectedPlaceText = string.Empty;
            FoundPlaces = [];
            FoundPlacesSelectionTexts = [];
        }

        private async Task<List<(EDynamicPrayerTimeProviderType, BaseLocationData)>> getDynamicPrayerTimeProviderWithLocationData(ProfilePlaceInfo completePlaceInfo, CancellationToken cancellationToken)
        {
            var locationDataWithDynamicPrayerTimeProvider = new List<(EDynamicPrayerTimeProviderType, BaseLocationData)>();

            foreach (var dynamicPrayerTimeProvider in Enum.GetValues<EDynamicPrayerTimeProviderType>())
            {
                if (dynamicPrayerTimeProvider == EDynamicPrayerTimeProviderType.None)
                    continue;

                BaseLocationData locationConfig =
                    await prayerTimeServiceFactory
                        .GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(dynamicPrayerTimeProvider)
                        .GetLocationInfo(completePlaceInfo, cancellationToken);

                locationDataWithDynamicPrayerTimeProvider.Add((dynamicPrayerTimeProvider, locationConfig));
            }

            return locationDataWithDynamicPrayerTimeProvider;
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
