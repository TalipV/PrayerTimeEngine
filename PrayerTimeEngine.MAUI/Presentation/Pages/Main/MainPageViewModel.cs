using MetroLog.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Pages.DatabaseTables;
using PrayerTimeEngine.Presentation.Pages.Settings.SettingsHandler;
using PrayerTimeEngine.Presentation.Services;
using PrayerTimeEngine.Presentation.Services.Navigation;
using PrayerTimeEngine.Presentation.Views;
using PrayerTimeEngine.Services.PrayerTimeSummaryNotification;
using PropertyChanged;
using System.Windows.Input;

namespace PrayerTimeEngine.Presentation.Pages.Main;

[AddINotifyPropertyChangedInterface]
public class MainPageViewModel(
        IDispatcher dispatcher,
        IBrowser browser,
        ToastMessageService toastMessageService,
        ISystemInfoService systemInfoService,
        IDynamicPrayerTimeProviderFactory prayerTimeServiceFactory,
        IMosquePrayerTimeProviderManager mosquePrayerTimeProviderManager,
        PrayerTimeViewModelFactory prayerTimeViewModelFactory,
        NotificationService prayerTimeNotificationManager,
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

    public bool IsLoadingSelectedPlace { get; set; }

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

            string languageCode = systemInfoService.GetSystemCulture().TwoLetterISOLanguageName;
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

    public async Task CreateNewProfile()
    {
        try
        {
            CancellationToken cancellationToken = default;

            var copiedProfile = await profileService.CopyProfile(CurrentProfile, cancellationToken);
            await reloadProfiles(copiedProfile.ID);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while creating a new profile");
            toastMessageService.ShowError(exception.Message);
        }
    }

    public async Task CreateNewMosqueProfile(EMosquePrayerTimeProviderType selectedItem, string externalID)
    {
        try
        {
            CancellationToken cancellationToken = default;

            bool isValid = await mosquePrayerTimeProviderManager.ValidateData(selectedItem, externalID, cancellationToken);

            if (!isValid)
            {
                return;
            }

            var newMosqueProfile = await profileService.CreateNewMosqueProfile(selectedItem, externalID, cancellationToken);
            await reloadProfiles(newMosqueProfile.ID);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while creating a new mosque profile");
            toastMessageService.ShowError(exception.Message);
        }
    }

    public async Task DeleteCurrentProfile()
    {
        try
        {
            CancellationToken cancellationToken = default;

            await profileService.DeleteProfile(CurrentProfile, cancellationToken);
            await reloadProfiles();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while deleting current profile");
            toastMessageService.ShowError(exception.Message);
        }
    }

    public async Task ShowDatabaseTable()
    {
        await navigationService.NavigateTo<DatabaseTablesPageViewModel>();
    }

    public string GetPrayerTimeConfigDisplayText()
    {
        try
        {
            if (CurrentProfile is not DynamicProfile dynamicProfile)
            {
                return $"Not supported for '{CurrentProfile?.GetType().Name ?? "NULL"}'!";
            }

            return profileService.GetPrayerTimeConfigDisplayText(dynamicProfile);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while deleting current profile");
            toastMessageService.ShowError(exception.Message);

            return $"ERROR: {exception.Message}";
        }
    }

    public string GetLocationDataDisplayText()
    {
        try
        {
            if (CurrentProfile is not DynamicProfile dynamicProfile)
            {
                return $"Not supported for '{CurrentProfile?.GetType().Name ?? "NULL"}'!";
            }

            return profileService.GetLocationDataDisplayText(dynamicProfile);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while deleting current profile");
            toastMessageService.ShowError(exception.Message);

            return $"ERROR: {exception.Message}";
        }
    }

    public Task ChangeProfileName(string newProfileName)
    {
        CancellationToken cancellationToken = default;

        try
        {
            if (CurrentProfile != null)
            {
                Task task = profileService.ChangeProfileName(CurrentProfile, newProfileName, cancellationToken);
                OnPropertyChanged(nameof(CurrentProfile));
                return task;
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while changing the name of a profile");
            toastMessageService.ShowError(exception.Message);
        }

        return Task.CompletedTask;
    }

    public Task OpenMosqueInternetPage()
    {
        if (CurrentProfile is not MosqueProfile mosque)
            return Task.CompletedTask;

        string url = mosque.MosqueProviderType switch
        {
            EMosquePrayerTimeProviderType.Mawaqit => $"https://mawaqit.net/de/{mosque.ExternalID}",
            EMosquePrayerTimeProviderType.MyMosq => $"https://mymosq.com/mosque/{mosque.ExternalID}",
            _ => null
        };

        if (string.IsNullOrWhiteSpace(url))
            return Task.CompletedTask;

        return browser.OpenAsync(new Uri(url));
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
            ProfilesWithModel = profiles.Select(prayerTimeViewModelFactory.Create).ToList();
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

                DateTimeZone dateTimeZone = profileService.GetDateTimeZone(currentProfile);
                ZonedDateTime zonedDateTime =
                    systemInfoService.GetCurrentInstant()
                        .InZone(dateTimeZone);

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

            dispatcher.Dispatch(async () => await prayerTimeNotificationManager.StartPrayerTimeSummaryNotification());
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

            DynamicProfile currentProfile = CurrentProfile as DynamicProfile;

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

    #endregion private methods
}
