using System.Globalization;
using System.Windows.Input;
using MetroLog.Maui;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.IslamicCalendar.Interfaces;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Pages.DatabaseTables;
using PrayerTimeEngine.Presentation.Pages.Logs;
using PrayerTimeEngine.Presentation.Pages.Settings.SettingsHandler;
using PrayerTimeEngine.Presentation.Services;
using PrayerTimeEngine.Presentation.Services.Navigation;
using PrayerTimeEngine.Presentation.Views;
using PrayerTimeEngine.Presentation.Views.PrayerTimes;
using PrayerTimeEngine.Services;
using PrayerTimeEngine.Services.Notifications;
using PropertyChanged;
using PrayerTimeEngine.Presentation.Localization;

namespace PrayerTimeEngine.Presentation.Pages.Main;

[AddINotifyPropertyChangedInterface]
public partial class MainPageViewModel(
        IDispatcher dispatcher,
        IBrowser browser,
        ToastMessageService toastMessageService,
        FavoritePlacesService favoritePlacesService,
        ProfileOverviewTextService profileOverviewTextService,
        ISystemInfoService systemInfoService,
        IMosquePrayerTimeProviderManager mosquePrayerTimeProviderManager,
        PrayerTimeViewModelFactory prayerTimeViewModelFactory,
        NotificationService prayerTimeNotificationManager,
        WelcomeService welcomeService,
        IPlaceService placeService,
        IProfileService profileService,
        INavigationService navigationService,
        IIslamicDateCalculationService islamicDateCalculationService,
        IAppInitializer appInitializer,
        ILogger<MainPageViewModel> logger
    ) : LogController
{
    #region fields

    private Debounce.Core.Debouncer _debouncer;
    private bool _isSettingsPageOpening = false;

    #endregion fields

    #region properties

    public List<IPrayerTimeViewModel> ProfilesWithModel { get; set; }

    [OnChangedMethod(nameof(OnCurrentProfileWithModelChanged))]
    public IPrayerTimeViewModel CurrentProfileWithModel { get; set; }
    public Profile CurrentProfile
    {
        get
        {
            return CurrentProfileWithModel?.Profile;
        }
    }

    public bool IsLoadingPrayerTimes { get; set; }

    public bool IsLoadingPrayerTimesOrSelectedPlace => IsLoadingPrayerTimes || IsLoadingSelectedPlace;

    public bool IsLoadingSelectedPlace { get; set; }

    /// <summary>Text of the loading indicator on the main page, naming what is currently being loaded.</summary>
    public string LoadingStatusText =>
        IsLoadingSelectedPlace
            ? AppResources.MainPage_LoadingPlace
            : AppResources.MainPage_LoadingPrayerTimes;

    public bool IsSearchBoxEnabled
    {
        get
        {
            return !IsLoadingPrayerTimesOrSelectedPlace && CurrentProfile is DynamicProfile;
        }
    }

    public bool IsMainPageEnabled { get; private set; } = false;

    [OnChangedMethod(nameof(OnPlaceSearchTextChanged))]
    public string PlaceSearchText { get; set; }

    /// <summary>Rows shown in the custom search dropdown: favorites, a separator, then normal results.</summary>
    public List<PlaceSearchResultItem> SearchItems { get; set; } = [];

    /// <summary>Whether the search dropdown is currently shown below the search field.</summary>
    public bool IsSearchDropdownVisible { get; set; }

    // last normal search results, kept so focusing the field can re-show the dropdown without re-searching
    private List<BasicPlaceInfo> _lastResults = [];

    #region selected date

    /// <summary>
    /// The day chosen in the date bar; <c>null</c> means the current day, which also keeps following
    /// the current day when the date changes while the app is open.
    /// </summary>
    public LocalDate? SelectedDate { get; private set; }

    /// <summary>When the date was last chosen. After a longer break the app returns to the current day.</summary>
    private Instant _selectedDateChangedAt;

    private static readonly Duration s_selectedDateResetAfter = Duration.FromMinutes(15);

    public bool IsCurrentDaySelected => SelectedDate is null;

    /// <summary>The day that is shown (or about to be shown) for the current profile.</summary>
    public LocalDate DisplayedDate => SelectedDate ?? GetCurrentDate();

    /// <summary>Text of the date bar, e.g. "Heute, Sa. 21.11.2026" or "Mi. 3.12.2026".</summary>
    public string SelectedDateText
    {
        get
        {
            LocalDate currentDate = GetCurrentDate();
            LocalDate displayedDate = DisplayedDate;
            string dateText = displayedDate.ToString("ddd dd.MM.yyyy", CultureInfo.CurrentUICulture);

            int dayDifference = Period.Between(currentDate, displayedDate, PeriodUnits.Days).Days;
            string? relativeDayText = dayDifference switch
            {
                0 => AppResources.MainPage_Date_Today,
                -1 => AppResources.MainPage_Date_Yesterday,
                1 => AppResources.MainPage_Date_Tomorrow,
                _ => null,
            };

            return relativeDayText is null ? dateText : $"{relativeDayText}, {dateText}";
        }
    }

    /// <summary>Two-way bound to the date picker of the date bar.</summary>
    public DateTime SelectedDateTime
    {
        get => DisplayedDate.ToDateTimeUnspecified();
        set => SelectDate(LocalDate.FromDateTime(value));
    }

    public ICommand PreviousDayCommand => new Command(() => SelectDate(DisplayedDate.PlusDays(-1)));
    public ICommand NextDayCommand => new Command(() => SelectDate(DisplayedDate.PlusDays(1)));
    public ICommand CurrentDayCommand => new Command(() => SelectDate(null));

    private LocalDate GetCurrentDate()
    {
        DateTimeZone dateTimeZone = CurrentProfile is Profile profile
            ? profileService.GetDateTimeZone(profile)
            : systemInfoService.GetSystemTimeZone();

        return systemInfoService.GetCurrentInstant().InZone(dateTimeZone).Date;
    }

    private void SelectDate(LocalDate? date)
    {
        // choosing the current day means following the current day
        if (date == GetCurrentDate())
        {
            date = null;
        }

        _selectedDateChangedAt = systemInfoService.GetCurrentInstant();

        if (date == SelectedDate)
        {
            return;
        }

        SelectedDate = date;
        NotifySelectedDateChanged();

        Task.Run(RefreshData);
    }

    private void NotifySelectedDateChanged()
    {
        OnPropertyChanged(nameof(IsCurrentDaySelected));
        OnPropertyChanged(nameof(DisplayedDate));
        OnPropertyChanged(nameof(SelectedDateText));
        OnPropertyChanged(nameof(SelectedDateTime));
    }

    #endregion selected date

    #region loading problems

    /// <summary>
    /// What went wrong while loading the shown times, e.g. a provider that failed or has no times for the day.
    /// Shown as a warning sign; tapping it lists the problems.
    /// </summary>
    public List<LoadingProblem> LoadingProblems { get; private set; } = [];

    public bool HasLoadingProblems => LoadingProblems.Count != 0;

    /// <summary>The problems as text for the popup, grouped by source and message, with the affected days.</summary>
    public string LoadingProblemsText =>
        string.Join(
            Environment.NewLine + Environment.NewLine,
            LoadingProblems
                .GroupBy(x => (x.Source, x.Message))
                .OrderBy(x => x.Key.Source)
                .Select(group =>
                {
                    string dates = string.Join(
                        ", ",
                        group
                            .Where(x => x.Date is not null)
                            .Select(x => x.Date.Value)
                            .Distinct()
                            .Order()
                            .Select(x => x.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)));

                    string header = string.IsNullOrEmpty(dates) ? group.Key.Source : $"{group.Key.Source} ({dates})";
                    return $"{header}:{Environment.NewLine}{group.Key.Message}";
                }));

    private void SetLoadingProblems(List<LoadingProblem> loadingProblems)
    {
        LoadingProblems = loadingProblems;
        OnPropertyChanged(nameof(HasLoadingProblems));
        OnPropertyChanged(nameof(LoadingProblemsText));
    }

    private static string GetLoadingProblemMessage(Exception exception)
    {
        return exception switch
        {
            PrayerTimesNotAvailableForDateException => AppResources.MainPage_Problem_NotAvailable,

            // Polly's circuit breaker (without referencing Polly): too many requests failed shortly before
            _ when exception.GetType().Name == "BrokenCircuitException"
                || exception.InnerException?.GetType().Name == "BrokenCircuitException"
                => string.Format(AppResources.MainPage_Problem_CircuitOpen, exception.Message),

            _ => exception.Message,
        };
    }

    #endregion loading problems

    public string WeeksUntilLabelText
    {
        get
        {
            int weeksUntilRamadan = islamicDateCalculationService.GetWeeksUntilRamadan();
            int weeksUntilHajjSeason = islamicDateCalculationService.GetWeeksUntilHajj();

            List<(int WeeksCount, string Text)> weeksUntilLabelLines = [];
            weeksUntilLabelLines.Add((weeksUntilHajjSeason, string.Format(AppResources.MainPage_WeeksUntilHajj, weeksUntilHajjSeason)));
            weeksUntilLabelLines.Add((weeksUntilRamadan, string.Format(AppResources.MainPage_WeeksUntilRamadan, weeksUntilRamadan)));
            return string.Join(
                Environment.NewLine,
                weeksUntilLabelLines
                    .Where(x => x.WeeksCount < 25)
                    .OrderByDescending(x => x.WeeksCount)
                    .Select(x => x.Text));
        }
    }

    #endregion properties

    #region ICommand

    public ICommand GoToSettingsPageCommand
            => new Command<ETimeSection>(
                async (section) =>
                {
                    if (_isSettingsPageOpening)
                    {
                        return;
                    }

                    // stop loading times and stop loading places
                    _loadingTimesCancellationTokenSource?.Cancel();
                    _placeSearchCancellationTokenSource?.Cancel();

                    try
                    {
                        _isSettingsPageOpening = true;
                        await navigationService.NavigateTo<SettingsHandlerPageViewModel>(CurrentProfile, section);
                    }
                    finally
                    {
                        _isSettingsPageOpening = false;
                    }
                });

    public event Action OnAfterLoadingPrayerTimes_EventTrigger = delegate { };

    #endregion ICommand

    #region public methods

    public async void OnPlaceSearchTextChanged()
    {
        if (!IsValidPlaceSearchText())
        {
            // no (valid) search text yet: drop old results but still offer favorites
            _lastResults = [];
            await BuildSearchItemsAsync([]);
            return;
        }

        _debouncer ??= new Debounce.Core.Debouncer(SearchPlace, intervalMs: 600);
        _debouncer.Debounce();
    }

    /// <summary>Called when the search field gets focus: (re)builds and shows the dropdown (favorites first).</summary>
    public async Task OnSearchFocused()
    {
        await BuildSearchItemsAsync(_lastResults);
    }

    /// <summary>Called when the search field loses focus. Delayed so a row tap can still register first.</summary>
    public void OnSearchUnfocused()
    {
        dispatcher.DispatchDelayed(
            TimeSpan.FromMilliseconds(200),
            () => IsSearchDropdownVisible = false);
    }

    /// <summary>Closes the suggestion dropdown (e.g. when the user taps outside of it).</summary>
    public void CloseSearchDropdown()
    {
        IsSearchDropdownVisible = false;
    }

    /// <summary>Removes a place from the favorites and refreshes the dropdown so the row disappears.</summary>
    public async Task RemoveFavorite(BasicPlaceInfo place)
    {
        if (place is null)
        {
            return;
        }

        try
        {
            await favoritePlacesService.RemoveAsync(place, cancellationToken: default);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while removing a favorite place");
            toastMessageService.ShowError(exception.Message);
        }

        await BuildSearchItemsAsync(_lastResults);
    }

    public ICommand SelectPlaceCommand
        => new Command<PlaceSearchResultItem>(item =>
        {
            if (item is null || item.IsSeparator || item.Place is null)
            {
                return;
            }

            IsSearchDropdownVisible = false;
            SelectPlace(item.Place);
        });

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
            {
                _placeSearchCancellationTokenSource = null;
            }
        }

        return [];
    }

    public async Task OnActualAppearing()
    {
        // after a longer break the app shows the current day again instead of a day chosen back then
        if (SelectedDate is not null
            && systemInfoService.GetCurrentInstant() - _selectedDateChangedAt > s_selectedDateResetAfter)
        {
            SelectedDate = null;
        }

        // also updates relative texts like "today" after the date changed while the app was in the background
        await dispatcher.DispatchAsync(NotifySelectedDateChanged);

        await RefreshData();
    }

    public async Task CreateNewProfile()
    {
        try
        {
            CancellationToken cancellationToken = default;

            Profile copiedProfile = await profileService.CopyProfile(CurrentProfile, cancellationToken);
            await ReloadProfiles(copiedProfile.ID);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while creating a new profile");
            toastMessageService.ShowError(exception.Message);
        }
    }

    public async Task CreateNewMosqueProfile(EMosquePrayerTimeProviderType providerType, string externalID)
    {
        try
        {
            CancellationToken cancellationToken = default;

            // e.g. Mawaqit's numeric "id/11504" from the share text becomes the slug "assunnah-frechen", together
            // with the mosque's name from the mosque app (independent of any language); null when the mosque does not exist
            MosqueIdentity? mosqueIdentity = await mosquePrayerTimeProviderManager.GetMosqueIdentityAsync(providerType, externalID, cancellationToken);

            if (mosqueIdentity is null)
            {
                toastMessageService.ShowError(string.Format(AppResources.MosqueProfile_NotFound, providerType));
                return;
            }

            MosqueProfile newMosqueProfile = await profileService.CreateNewMosqueProfile(providerType, mosqueIdentity.ExternalID, mosqueIdentity.Name, cancellationToken);
            await ReloadProfiles(newMosqueProfile.ID);
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
            await ReloadProfiles();
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

    public async Task ShowLogs()
    {
        await navigationService.NavigateTo<LogsPageViewModel>();
    }

    public string GetPrayerTimeConfigDisplayText()
    {
        try
        {
            if (CurrentProfile is not DynamicProfile dynamicProfile)
            {
                return string.Format(AppResources.Common_NotSupportedForProfile, CurrentProfile?.GetType().Name ?? "NULL");
            }

            return profileOverviewTextService.GetTimeConfigurationText(dynamicProfile);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while deleting current profile");
            toastMessageService.ShowError(exception.Message);

            return string.Format(AppResources.Common_ErrorWithMessage, exception.Message);
        }
    }

    public string GetLocationDataDisplayText()
    {
        try
        {
            if (CurrentProfile is not DynamicProfile dynamicProfile)
            {
                return string.Format(AppResources.Common_NotSupportedForProfile, CurrentProfile?.GetType().Name ?? "NULL");
            }

            return profileOverviewTextService.GetLocationDataText(dynamicProfile);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while deleting current profile");
            toastMessageService.ShowError(exception.Message);

            return string.Format(AppResources.Common_ErrorWithMessage, exception.Message);
        }
    }

    /// <summary>
    /// The coordinates the location overview shows (those of Muwaqqit, the only source working with
    /// coordinates), or <c>null</c> when the current profile has none.
    /// </summary>
    public (decimal Latitude, decimal Longitude)? GetLocationCoordinates()
    {
        if (CurrentProfile is not DynamicProfile dynamicProfile
            || profileService.GetLocationConfig(dynamicProfile, EDynamicPrayerTimeProviderType.Muwaqqit) is not MuwaqqitLocationData muwaqqitLocationData)
        {
            return null;
        }

        return (muwaqqitLocationData.Latitude, muwaqqitLocationData.Longitude);
    }

    /// <summary>
    /// Opens the coordinates in the map app of the device. The geo URI lets the user pick any installed
    /// map app; Google Maps in the browser is the fallback for devices without one.
    /// </summary>
    public async Task OpenCoordinatesInMapApp(decimal latitude, decimal longitude)
    {
        string coordinates = string.Create(CultureInfo.InvariantCulture, $"{latitude},{longitude}");

        if (await Launcher.Default.TryOpenAsync(new Uri($"geo:{coordinates}?q={coordinates}")))
        {
            return;
        }

        await browser.OpenAsync(new Uri($"https://www.google.com/maps/search/?api=1&query={coordinates}"));
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
        {
            return Task.CompletedTask;
        }

        string url = mosque.MosqueProviderType switch
        {
            EMosquePrayerTimeProviderType.Mawaqit => $"https://mawaqit.net/de/{mosque.ExternalID}",
            EMosquePrayerTimeProviderType.MyMosq => $"https://mymosq.com/mosque/{mosque.ExternalID}",
            _ => null
        };

        if (string.IsNullOrWhiteSpace(url))
        {
            return Task.CompletedTask;
        }

        return browser.OpenAsync(new Uri(url));
    }

    public async Task ReloadAfterConfigurationImport()
    {
        await ReloadProfiles();
        await RefreshData();
    }

    #endregion public methods

    #region private methods

    private async void SearchPlace()
    {
        if (!IsValidPlaceSearchText())
        {
            return;
        }

        List<BasicPlaceInfo> results = await PerformPlaceSearch(PlaceSearchText);
        await BuildSearchItemsAsync(results);
    }

    /// <summary>
    /// Loads the favorites from the database and then builds the dropdown rows on the UI thread.
    /// </summary>
    private async Task BuildSearchItemsAsync(List<BasicPlaceInfo> results)
    {
        List<FavoritePlace> favorites = await favoritePlacesService.GetFavoritesAsync(cancellationToken: default);

        // building touches bound collections, so make sure it runs on the UI thread
        await dispatcher.DispatchAsync(() => BuildSearchItems(results, favorites));
    }

    /// <summary>
    /// Builds the dropdown rows: matching favorites first, a separator, then the normal results.
    /// With no search text, all favorites are offered so they are available right at the start of a search.
    /// </summary>
    private void BuildSearchItems(List<BasicPlaceInfo> results, List<FavoritePlace> favorites)
    {
        _lastResults = results ?? [];
        string search = (PlaceSearchText ?? string.Empty).Trim();

        List<FavoritePlace> shownFavorites =
            string.IsNullOrWhiteSpace(search)
                ? favorites
                : [.. favorites.Where(f => f.DisplayText.Contains(search, StringComparison.OrdinalIgnoreCase))];

        HashSet<string> favoriteKeys = [.. shownFavorites.Select(f => f.DisplayText)];
        List<BasicPlaceInfo> normalResults =
            [.. _lastResults
                .Where(r => !favoriteKeys.Contains(r.DisplayText))
                .GroupBy(r => r.DisplayText)
                .Select(g => g.First())
                .OrderBy(r => r.DisplayText)
                .Take(7)];

        var items = new List<PlaceSearchResultItem>();

        foreach (FavoritePlace favorite in shownFavorites)
        {
            items.Add(new PlaceSearchResultItem { DisplayText = favorite.DisplayText, IsFavorite = true, Place = favorite });
        }

        if (shownFavorites.Count > 0 && normalResults.Count > 0)
        {
            items.Add(new PlaceSearchResultItem { IsSeparator = true, DisplayText = AppResources.MainPage_SearchResults });
        }

        foreach (BasicPlaceInfo result in normalResults)
        {
            items.Add(new PlaceSearchResultItem { DisplayText = result.DisplayText, IsFavorite = false, Place = result });
        }

        SearchItems = items;
        IsSearchDropdownVisible = items.Count > 0;
    }

    private bool IsValidPlaceSearchText()
    {
        return !string.IsNullOrWhiteSpace(PlaceSearchText)
            && PlaceSearchText.Replace(" ", string.Empty).Length > 4;
    }

    private async Task ReloadProfiles(int selectedProfile = 1)
    {
        List<Profile> profiles = await profileService.GetProfiles(cancellationToken: default);

        bool oldValue = _suspendOnCurrentProfileWithModelChanged;
        try
        {
            _suspendOnCurrentProfileWithModelChanged = true;
            ProfilesWithModel = [.. profiles.Select(prayerTimeViewModelFactory.Create)];
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
    private long _isLoadPrayerTimesRunningInterlockedInt = 0;  // 0 for false, 1 for true
    private CancellationTokenSource _loadingTimesCancellationTokenSource;
    private bool _firstLoadDone = false;
    private bool _wasColdStart = false;

    private async Task RefreshData()
    {
        string refreshCallID = DebugUtil.GenerateDebugID();
        logger.LogInformation("Refreshing data started. ({RefreshCallID})", refreshCallID);

        IPrayerTimeViewModel? refreshedProfileWithModel = null;
        bool wasCanceled = false;

        try
        {
            if (!appInitializer.IsInitialized)
            {
                await OnBeforeFirstLoad();
            }

            try
            {
                Profile? currentProfile = CurrentProfile;

                if (currentProfile is null || Interlocked.CompareExchange(ref _isLoadPrayerTimesRunningInterlockedInt, 1, 0) == 1)
                {
                    return;
                }

                IsLoadingPrayerTimes = true;

                _loadingTimesCancellationTokenSource?.Cancel();
                _loadingTimesCancellationTokenSource?.Dispose();
                _loadingTimesCancellationTokenSource = new CancellationTokenSource();

                DateTimeZone dateTimeZone = profileService.GetDateTimeZone(currentProfile);
                ZonedDateTime now = systemInfoService.GetCurrentInstant().InZone(dateTimeZone);
                LocalDate? selectedDate = SelectedDate;

                ZonedDateTime zonedDateTime = selectedDate is LocalDate date && date != now.Date
                    ? date.AtStartOfDayInZone(dateTimeZone)
                    : now;

                refreshedProfileWithModel = CurrentProfileWithModel;
                await refreshedProfileWithModel.RefreshData(zonedDateTime, isCurrentDay: zonedDateTime.Date == now.Date, _loadingTimesCancellationTokenSource.Token);
                SetLoadingProblems(GetLoadingProblems(refreshedProfileWithModel));

                OnAfterLoadingPrayerTimes_EventTrigger?.Invoke();
            }
            finally
            {
                Interlocked.Exchange(ref _isLoadPrayerTimesRunningInterlockedInt, 0);  // Reset the flag to allow future runs
                IsLoadingPrayerTimes = false;
            }
        }
        catch (OperationCanceledException)
        {
            wasCanceled = true;
            logger.LogInformation("Loading times was canceled. ({RefreshCallID})", refreshCallID);
        }
        catch (PrayerTimesNotAvailableForDateException exception)
        {
            logger.LogInformation("{ProviderName} provides no times for {Date}. ({RefreshCallID})", exception.ProviderName, exception.Date, refreshCallID);
            SetLoadingProblems([new LoadingProblem(exception.ProviderName, exception.Date, GetLoadingProblemMessage(exception))]);
        }
        catch (Exception exception) when (refreshedProfileWithModel?.Profile is MosqueProfile mosqueProfile)
        {
            // the mosque app failed: shown behind the warning sign like the failures of the calculation sources
            logger.LogError(exception,
                "Error while loading the mosque times. ({RefreshCallID})", refreshCallID);
            SetLoadingProblems([new LoadingProblem(mosqueProfile.MosqueProviderType.ToString(), refreshedProfileWithModel.DisplayedDate, GetLoadingProblemMessage(exception))]);
        }
        catch (Exception exception)
        {
            logger.LogError(exception,
                "Error during refreshData. ({RefreshCallID})", refreshCallID);
            toastMessageService.ShowError(exception.Message);
        }
        finally
        {
            // has to happen even when the loading sequence failed or was skipped because
            // otherwise the main page stays disabled and the app is stuck on the loading screen
            if (!_firstLoadDone)
            {
                _firstLoadDone = true;
                OnAfterFirstLoad();
            }
        }

        logger.LogInformation("Refreshing data finished. ({RefreshCallID})", refreshCallID);

        // another day was chosen while the times were loading (that request was skipped), so load that day now
        if (!wasCanceled
            && refreshedProfileWithModel is not null
            && refreshedProfileWithModel == CurrentProfileWithModel
            && refreshedProfileWithModel.DisplayedDate != DisplayedDate)
        {
            await RefreshData();
        }
    }

    private static List<LoadingProblem> GetLoadingProblems(IPrayerTimeViewModel prayerTimeViewModel)
    {
        // the mosque times either load completely or throw (handled in RefreshData)
        if (prayerTimeViewModel is not DynamicPrayerTimeViewModel dynamicPrayerTimeViewModel)
        {
            return [];
        }

        // the neighbouring days are part of it as well: the times around midnight (e.g. the end of Isha) come from them
        return
            [.. (dynamicPrayerTimeViewModel.CalculationErrors ?? [])
                .Select(x => new LoadingProblem(
                    x.DynamicPrayerTimeProviderType.ToString(),
                    x.Date,
                    GetLoadingProblemMessage(x.Exception)))];
    }

    private async Task OnBeforeFirstLoad()
    {
        // the AppInitializer is a singleton, so it is only uninitialized when this process is fresh,
        // which means this is a real app start and not just a recreated activity of a long living process
        _wasColdStart = !appInitializer.IsInitialized;
        await appInitializer.InitializeAsync();

        bool oldValue = _suspendOnCurrentProfileWithModelChanged;
        try
        {
            _suspendOnCurrentProfileWithModelChanged = true;
            await ReloadProfiles();
        }
        finally
        {
            _suspendOnCurrentProfileWithModelChanged = oldValue;
        }
    }

    private void OnAfterFirstLoad()
    {
        try
        {
            // initial value is false and things like scroll actions before the first
            // loading sequence is through lead to weird exceptions from within the MAUI CarouselView code itself
            // so blocking the UI generally prevents edge cases like that
            IsMainPageEnabled = true;

            double startUpTimeMS = (DateTime.Now - MauiProgram.StartDateTime).TotalMilliseconds;

            // the duration limit covers the case of the process being restarted headlessly (e.g. by the
            // summary notification service) and the user only entering the app much later, which would
            // otherwise report an astronomically high start up time
            if (_wasColdStart && startUpTimeMS < 60_000)
            {
                toastMessageService.Show(string.Format(AppResources.MainPage_StartUpTime, startUpTimeMS.ToString("N0")));
            }
            dispatcher.Dispatch(async () =>
            {
                // the welcome notice comes first, before the notification permission and the battery optimization hint
                try
                {
                    await welcomeService.ShowIfNotConfirmedAsync();
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Error while showing the welcome notice");
                }

                await prayerTimeNotificationManager.StartPrayerTimeSummaryNotification();
            });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error during onAfterFirstLoad");
            toastMessageService.ShowError(exception.Message);
        }
    }

    // TODO use different approach
    private bool _suspendOnCurrentProfileWithModelChanged = false;

    private async void OnCurrentProfileWithModelChanged()
    {
        OnPropertyChanged(nameof(IsSearchBoxEnabled));

        if (_suspendOnCurrentProfileWithModelChanged || CurrentProfileWithModel == null)
        {
            return;
        }

        await RefreshData();
    }

    private void SelectPlace(BasicPlaceInfo selectedPlace)
    {
        if (selectedPlace is null)
        {
            return;
        }

        logger.LogInformation("Place '{SelectedPlace}' was selected", selectedPlace.DisplayText);

        Task.Run(async () =>
        {
            if (CurrentProfile is not DynamicProfile currentProfile)
            {
                return;
            }

            await dispatcher.DispatchAsync(() => ResetPlaceInput());

            try
            {
                IsLoadingSelectedPlace = true;

                ProfilePlaceInfo completePlaceInfo = await placeService.GetTimezoneInfo(selectedPlace, cancellationToken: default);

                await profileService.UpdateLocationConfig(
                    currentProfile,
                    completePlaceInfo,
                    cancellationToken: default);

                // notify UI for updates
                await dispatcher.DispatchAsync(() => OnPropertyChanged(nameof(CurrentProfile)));

                var locationConfigDynamicPrayerTimeProviders =
                    currentProfile.LocationConfigs
                        .Where(x => x.LocationData != null)
                        .Select(x => x.DynamicPrayerTimeProvider)
                        .ToHashSet();
                var missingLocationInfo =
                    Enum.GetValues<EDynamicPrayerTimeProviderType>()
                        .Where(enumValue => enumValue != EDynamicPrayerTimeProviderType.None && !locationConfigDynamicPrayerTimeProviders.Contains(enumValue))
                        .OrderBy(x => x.ToString())
                        .ToList();
                if (missingLocationInfo.Count != 0)
                {
                    logger.LogWarning("Location information missing for the following calculation sources: {@DynamicPrayerTimeProviders}", missingLocationInfo);
                    toastMessageService.ShowWarning(string.Format(AppResources.MainPage_LocationInformationMissing, string.Join(", ", missingLocationInfo)));
                }

                // offer to save the just selected place as a favorite (only if it is not one already)
                if (!await favoritePlacesService.IsFavoriteAsync(selectedPlace, cancellationToken: default))
                {
                    toastMessageService.ShowAction(
                        text: string.Format(AppResources.MainPage_AddFavoriteQuestion, selectedPlace.City),
                        actionText: AppResources.MainPage_AddFavoriteAction,
                        action: async () =>
                        {
                            try
                            {
                                await favoritePlacesService.AddAsync(selectedPlace, cancellationToken: default);
                            }
                            catch (Exception exception)
                            {
                                logger.LogError(exception, "Error while adding a favorite place");
                                toastMessageService.ShowError(exception.Message);
                            }
                        },
                        duration: TimeSpan.FromSeconds(5));
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

            await RefreshData();
        });
    }

    private void ResetPlaceInput(bool resetPlaceSearchText = true)
    {
        if (resetPlaceSearchText)
        {
            PlaceSearchText = string.Empty;
        }

        _lastResults = [];
        SearchItems = [];
        IsSearchDropdownVisible = false;
    }

    #endregion private methods
}

/// <summary>A problem while loading the shown times: which source, for which day and what went wrong.</summary>
public record LoadingProblem(string Source, LocalDate? Date, string Message);
