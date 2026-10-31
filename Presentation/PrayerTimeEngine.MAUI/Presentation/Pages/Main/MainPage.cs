using System.ComponentModel;
using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.Shapes;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Views.MosquePrayerTimes;
using PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic;
using PrayerTimeEngine.Presentation.Views.PrayerTimes;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;
using PrayerTimeEngine.Presentation.Localization;

namespace PrayerTimeEngine.Presentation.Pages.Main;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;
    private readonly IDispatcher _dispatcher;
    private readonly IPreferenceService _preferenceService;
    private readonly ISystemInfoService _systemInfoService;
    private readonly MainPageOptionsMenuService _mainPageOptionsMenuService;

    public MainPage(
            IDispatcher dispatcher,
            MainPageViewModel viewModel,
            IPreferenceService preferenceService,
            ISystemInfoService systemInfoService,
            PrayerTimeGraphicView prayerTimeGraphicView
        )
    {
        BindingContext = _viewModel = viewModel;
        _dispatcher = dispatcher;
        _preferenceService = preferenceService;
        _systemInfoService = systemInfoService;

        // provide all constructor service params automatically through DI but add MainPage param ('this') manually
        _mainPageOptionsMenuService = ActivatorUtilities.CreateInstance<MainPageOptionsMenuService>(
                MauiProgram.ServiceProvider,
                this,
                viewModel
            );

        _prayerTimeGraphicView = prayerTimeGraphicView;

        if (_preferenceService.CheckAndResetDoReset()
            && File.Exists(AppConfig.DATABASE_PATH))
        {
            File.Delete(AppConfig.DATABASE_PATH);
        }

        Content = CreateUI();

        BackgroundColor = AppColors.Background;

        viewModel.OnAfterLoadingPrayerTimes_EventTrigger += ViewModel_OnAfterLoadingPrayerTimes_EventTrigger;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainPageViewModel.IsLoadingPrayerTimesOrSelectedPlace))
        {
            // the loading flags are set from background threads
            _dispatcher.Dispatch(() => _ = UpdateLoadingIndicatorAsync(_viewModel.IsLoadingPrayerTimesOrSelectedPlace));
        }
    }

    private void ViewModel_OnAfterLoadingPrayerTimes_EventTrigger()
    {
        _dispatcher.Dispatch(() =>
        {
            Instant instant = _systemInfoService.GetCurrentInstant();

            // For debugging
            //// 01:00 AM the following day
            //instant = _systemInfoService.GetCurrentZonedDateTime()
            //    .LocalDateTime.Date
            //    .AtStartOfDayInZone(_systemInfoService.GetSystemTimeZone())
            //    .PlusHours(25)
            //    .ToInstant();

            _prayerTimeGraphicView.PrayerTimeGraphicTime = _viewModel.CurrentProfileWithModel.CreatePrayerTimeGraphicTimeVO(instant);
            _prayerTimeGraphicViewBaseView.Invalidate();
        });
    }

    /// <summary>
    /// Triggers when the app is opened after being minimized
    /// </summary>
    private void App_Resumed()
    {
        Task.Run(_viewModel.OnActualAppearing);
    }

    /// <summary>
    /// Triggers when this page is navigated to from another page
    /// </summary>
    protected override void OnAppearing()
    {
        if (Application.Current is App app)
        {
            app.Resumed -= App_Resumed;
            app.Resumed += App_Resumed;
        }

        // resetting the title view like this was necessary to fix a MAUI bug
        // where the contents become invisible after navigating back to this page from another page
        NavigationPage.SetTitleView(this, null);
        NavigationPage.SetTitleView(this, _titleGrid);

        Task.Run(_viewModel.OnActualAppearing);
    }

    protected override void OnDisappearing()
    {
        if (Application.Current is App app)
        {
            app.Resumed -= App_Resumed;
        }
    }

    private Grid _titleGrid;
    private Label _weeksUntilTextInfo;
    private Label _profileDisplayNameTextInfo;
    private readonly PrayerTimeGraphicView _prayerTimeGraphicView;
    private GraphicsView _prayerTimeGraphicViewBaseView;
    private CarouselView _carouselView;
    private Entry _searchEntry;
    private Grid _contentGrid;
    private Border _loadingIndicator;
    private ActivityIndicator _loadingSpinner;

    private Grid CreateUI()
    {
        this.Bind<MainPage, bool, bool>(
            IsEnabledProperty,
            nameof(MainPageViewModel.IsMainPageEnabled),
            convert: value => value);

        _titleGrid = new Grid
        {
            ColumnDefinitions = Columns.Define(
                GridLength.Star,
                GridLength.Auto,
                GridLength.Auto)
        };

        _profileDisplayNameTextInfo = new Label()
            .Column(0).Row(0)
            .Start().CenterVertical()
            .FontSize(AppFontSizes.Title)
            .Font(bold: true)
            .TextColor(AppColors.Text)
            .Bind(Label.TextProperty, $"{nameof(MainPageViewModel.CurrentProfile)}.{nameof(Profile.Name)}");
        _profileDisplayNameTextInfo.LineBreakMode = LineBreakMode.TailTruncation;
        _titleGrid.Add(_profileDisplayNameTextInfo);

        _weeksUntilTextInfo = new Label()
            .Column(1).Row(0).Paddings(12, 0, 4, 0)
            .End().CenterVertical()
            .FontSize(AppFontSizes.Subtitle - 3)
            .TextColor(AppColors.TextMuted)
            .Bind(Label.TextProperty, $"{nameof(MainPageViewModel.WeeksUntilLabelText)}");
        _weeksUntilTextInfo.HorizontalTextAlignment = TextAlignment.End;
        _titleGrid.Add(_weeksUntilTextInfo);

        _titleGrid.Add(CreateOptionsMenuButton(), column: 2, row: 0);

        NavigationPage.SetTitleView(this, _titleGrid);

        // TODO fix: The grid doesn't fill the whole width when running on Windows.
        // Quick fix
#if WINDOWS
        _titleGrid.WidthRequest = 415;
#endif

        Border searchField = CreateSearchField();
        Border searchDropdown = CreateSearchDropdown();

        // position the dropdown directly below the search field. It is added spanning all rows
        // (see below) so it floats as an overlay and does not take up layout space / push content down.
        searchField.SizeChanged += (_, _) =>
        {
            if (searchField.Height > 0)
            {
                searchDropdown.Margin = new Thickness(0, searchField.Height + 2, 0, 0);
            }
        };

        Grid mainGrid = new Grid
        {
            RowDefinitions = Rows.Define(
                GridLength.Auto,
                new GridLength(11, GridUnitType.Star),
                new GridLength(7, GridUnitType.Star)
            )
        }
        .Paddings(10, 20, 10, 20);

        // Graphics View
        _prayerTimeGraphicViewBaseView = new GraphicsView
        {
            Drawable = _prayerTimeGraphicView
        };

        _carouselView = new CarouselView
        {
            ItemTemplate = new ProfileDataTemplateSelector
            {
                MosquePrayerTimeTemplate = new DataTemplate(() => new MosquePrayerTimeView(_systemInfoService)),
                PrayerTimesTemplate = new DataTemplate(() => new DynamicPrayerTimeView(_viewModel, _systemInfoService))
            },
            Loop = false
        }
        .FillHorizontal()
        .FillVertical()
        .Bind(ItemsView.ItemsSourceProperty, nameof(MainPageViewModel.ProfilesWithModel), mode: BindingMode.TwoWay)
        .Bind(CarouselView.CurrentItemProperty, nameof(MainPageViewModel.CurrentProfileWithModel), mode: BindingMode.TwoWay)
        .Bind<CarouselView, bool, bool>(
            CarouselView.IsSwipeEnabledProperty,
            nameof(MainPageViewModel.IsMainPageEnabled),
            convert: value => value,
            mode: BindingMode.TwoWay);

        // invisible full-page layer that catches taps outside the dropdown and closes it.
        // Needed because on Android tapping a non-focusable area does not unfocus the search Entry,
        // so the Unfocused event alone can't be relied on to dismiss the suggestions.
        Grid searchDismissScrim = new Grid
        {
            // a fully transparent color would not receive touches on all platforms, so use near-zero alpha
            BackgroundColor = Color.FromRgba(0, 0, 0, 0.01),
        }
        .Bind(IsVisibleProperty, nameof(MainPageViewModel.IsSearchDropdownVisible));
        searchDismissScrim.GestureRecognizers.Add(
            new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    _searchEntry?.Unfocus();
                    _viewModel.CloseSearchDropdown();
                })
            });

        mainGrid.AddWithSpan(_carouselView, row: 1, column: 0);
        mainGrid.AddWithSpan(_prayerTimeGraphicViewBaseView, row: 2, column: 0);
        mainGrid.AddWithSpan(searchDismissScrim, row: 0, column: 0, rowSpan: 3);
        mainGrid.AddWithSpan(searchField, row: 0, column: 0);
        // spans all rows so the overlay can extend over the content below the field without resizing row 0
        mainGrid.AddWithSpan(searchDropdown, row: 0, column: 0, rowSpan: 3);

        // stacking order: page content (0) < scrim (1) < search field (2) < dropdown overlay (3)
        // so taps on the field and the dropdown work, while taps anywhere else hit the scrim and dismiss it
        searchDismissScrim.ZIndex = 1;
        searchField.ZIndex = 2;
        searchDropdown.ZIndex = 3;

        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacCatalyst())
        {
            // swiping carousel view doesn't work on Windows so this workaround will have to do, at least for now
            _prayerTimeGraphicViewBaseView.GestureRecognizers
                .Add(
                    new TapGestureRecognizer
                    {
                        Command = new Command(() =>
                        {
                            // starting to interact too early leads to ItemsSource = null scenario
                            List<IPrayerTimeViewModel> viewModels = _carouselView.ItemsSource?.OfType<IPrayerTimeViewModel>().ToList() ?? [];
                            if (viewModels.Count == 0)
                            {
                                return;
                            }

                            var currentViewModel = _carouselView.CurrentItem as IPrayerTimeViewModel;
                            IPrayerTimeViewModel firstViewModel = viewModels.First();

                            int nextItemIndex = viewModels.IndexOf(currentViewModel ?? firstViewModel) + 1;
                            _carouselView.CurrentItem = viewModels.ElementAtOrDefault(nextItemIndex) ?? firstViewModel;
                        }),
                        NumberOfTapsRequired = 2,
                    });
        }

        _contentGrid = mainGrid;
        _loadingIndicator = CreateLoadingIndicator();

        // the indicator lives outside of the content grid, so dimming the content does not dim the indicator too
        return new Grid
        {
            Children = { mainGrid, _loadingIndicator }
        };
    }

    #region loading indicator

    /// <summary>
    /// Loads that finish faster than this never show the indicator, so cached data does not make the screen flicker.
    /// </summary>
    private const int LOADING_INDICATOR_DELAY_MS = 150;

    /// <summary>Once shown, the indicator stays at least this long, so it does not just flash up and vanish.</summary>
    private const int LOADING_INDICATOR_MIN_VISIBLE_MS = 450;

    private const uint LOADING_INDICATOR_FADE_MS = 180;

    /// <summary>Opacity of the page content while loading: clearly in the background, but still recognizable.</summary>
    private const double LOADING_CONTENT_OPACITY = 0.3;

    /// <summary>Incremented on every loading state change, so outdated delayed updates can tell they are outdated.</summary>
    private int _loadingIndicatorVersion;
    private long _loadingIndicatorShownAtMs;

    /// <summary>
    /// A pill in the middle of the page with an accent-coloured spinner and a text naming what is being loaded.
    /// It is input transparent, so it never changes what can be tapped while loading.
    /// </summary>
    private Border CreateLoadingIndicator()
    {
        _loadingSpinner = new ActivityIndicator
        {
            Color = AppColors.Accent,
            WidthRequest = 26,
            HeightRequest = 26,
            IsRunning = false,
            VerticalOptions = LayoutOptions.Center,
        };

        Label loadingLabel = new Label
        {
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = AppColors.Text,
            VerticalOptions = LayoutOptions.Center,
        }
        .Bind(Label.TextProperty, nameof(MainPageViewModel.LoadingStatusText));

        var loadingIndicator = new Border
        {
            BackgroundColor = AppColors.Surface,
            Stroke = AppColors.Accent,
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = 28 },
            Padding = new Thickness(22, 14, 26, 14),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.6f,
                Radius = 24,
                Offset = new Point(0, 6),
            },
            InputTransparent = true,
            IsVisible = false,
            Opacity = 0,
            Content = new HorizontalStackLayout
            {
                Spacing = 14,
                Children = { _loadingSpinner, loadingLabel }
            },
        };
        return loadingIndicator;
    }

    private async Task UpdateLoadingIndicatorAsync(bool isLoading)
    {
        int version = ++_loadingIndicatorVersion;

        if (isLoading)
        {
            if (_loadingIndicator.IsVisible)
            {
                // already shown (e.g. the place was loaded and now the prayer times follow): just make sure it is fully visible
                await ShowLoadingIndicatorAsync();
                return;
            }

            await Task.Delay(LOADING_INDICATOR_DELAY_MS);

            if (version != _loadingIndicatorVersion)
            {
                return;
            }

            _loadingIndicatorShownAtMs = Environment.TickCount64;
            await ShowLoadingIndicatorAsync();
        }
        else
        {
            if (!_loadingIndicator.IsVisible)
            {
                return;
            }

            long remainingMs = LOADING_INDICATOR_MIN_VISIBLE_MS - (Environment.TickCount64 - _loadingIndicatorShownAtMs);
            if (remainingMs > 0)
            {
                await Task.Delay((int)remainingMs);
            }

            if (version != _loadingIndicatorVersion)
            {
                return;
            }

            _loadingIndicator.CancelAnimations();
            _contentGrid.CancelAnimations();
            await Task.WhenAll(
                _loadingIndicator.FadeToAsync(0, LOADING_INDICATOR_FADE_MS, Easing.CubicIn),
                _contentGrid.FadeToAsync(1, LOADING_INDICATOR_FADE_MS, Easing.CubicIn));

            if (version == _loadingIndicatorVersion)
            {
                _loadingIndicator.IsVisible = false;
                _loadingSpinner.IsRunning = false;
            }
        }
    }

    private Task ShowLoadingIndicatorAsync()
    {
        _loadingSpinner.IsRunning = true;
        _loadingIndicator.IsVisible = true;

        _loadingIndicator.CancelAnimations();
        _contentGrid.CancelAnimations();
        return Task.WhenAll(
            _loadingIndicator.FadeToAsync(1, LOADING_INDICATOR_FADE_MS, Easing.CubicOut),
            _contentGrid.FadeToAsync(LOADING_CONTENT_OPACITY, LOADING_INDICATOR_FADE_MS, Easing.CubicOut));
    }

    #endregion loading indicator

    /// <summary>
    /// Visible entry point of the options menu at the right edge of the title bar, where Android apps
    /// usually put their overflow menu. Replaces the hidden double tap on the profile name.
    /// </summary>
    private View CreateOptionsMenuButton()
    {
        var menuIcon = new Label
        {
            Text = "⋮",
            FontSize = 26,
            FontAttributes = FontAttributes.Bold,
            TextColor = AppColors.Text,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
        };

        // the glyph itself is narrow, the surrounding area gives it a comfortable touch target
        var menuButton = new Grid
        {
            WidthRequest = 44,
            HeightRequest = 44,
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 8, 0),
            Children = { menuIcon }
        };
        SemanticProperties.SetDescription(menuButton, AppResources.MainPage_OptionsButton);

        menuButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await _mainPageOptionsMenuService.OpenOptionsMenu().ConfigureAwait(false))
        });

        return menuButton;
    }

    /// <summary>The search input field. Bound to the search text; opens/closes the dropdown on focus change.</summary>
    private Border CreateSearchField()
    {
        Entry searchEntry = new Entry
        {
            Placeholder = AppResources.MainPage_SearchPlaceholder,
            BackgroundColor = Colors.Transparent,
            TextColor = AppColors.Text,
            PlaceholderColor = AppColors.TextMuted,
            VerticalOptions = LayoutOptions.Center,
        }
        .Bind(Entry.TextProperty, nameof(MainPageViewModel.PlaceSearchText))
        .Bind<Entry, bool, bool>(
            IsEnabledProperty,
            nameof(MainPageViewModel.IsSearchBoxEnabled),
            convert: value => value);

        searchEntry.Focused += async (_, _) => await _viewModel.OnSearchFocused();
        searchEntry.Unfocused += (_, _) => _viewModel.OnSearchUnfocused();
        _searchEntry = searchEntry;

#if ANDROID
        // the field already sits in a rounded border, so the native underline of the EditText only adds noise
        searchEntry.HandlerChanged += (_, _) =>
        {
            if (searchEntry.Handler?.PlatformView is Android.Widget.EditText editText)
            {
                editText.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
            }
        };
#endif

        return new Border
        {
            BackgroundColor = AppColors.Card,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Padding = new Thickness(12, 0),
            HeightRequest = 44,
            VerticalOptions = LayoutOptions.Start,
            Content = searchEntry,
        };
    }

    /// <summary>
    /// The custom suggestion dropdown (favorites, a separator, then normal results).
    /// Rendered as a floating overlay - it is added to the grid spanning all rows and anchored to the
    /// top, so it never takes up layout space or pushes the prayer times down.
    /// A custom dropdown (instead of the native autocomplete) is also needed because the native Android
    /// autocomplete adapter filters the suggestion list itself, which would drop the favorites-vs-results
    /// separator row and any favorite that does not match the typed text.
    /// </summary>
    private Border CreateSearchDropdown()
    {
        // BindableLayout on a plain stack (no CollectionView, no ScrollView) so the overlay is exactly
        // as tall as the rows it actually shows - a ScrollView would grab a fixed/greedy height instead.
        // The list is small (favorites + separator + up to 7 results), so no virtualization/scroll is needed.
        var suggestionStack = new VerticalStackLayout { Spacing = 0 };
        BindableLayout.SetItemTemplate(suggestionStack, new DataTemplate(CreateSearchItemRow));
        suggestionStack.SetBinding(
            BindableLayout.ItemsSourceProperty,
            new Binding(nameof(MainPageViewModel.SearchItems)));

        return new Border
        {
            BackgroundColor = AppColors.Card,
            Stroke = AppColors.Border,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            // initial guess; corrected to the field's real height once it is measured (see CreateUI)
            Margin = new Thickness(0, 52, 0, 0),
            VerticalOptions = LayoutOptions.Start,
            Content = suggestionStack,
        }
        .Bind(IsVisibleProperty, nameof(MainPageViewModel.IsSearchDropdownVisible));
    }

    /// <summary>Builds one dropdown row: either a place row (with a star for favorites) or the separator row.</summary>
    private View CreateSearchItemRow()
    {
        // place row (favorite or normal result)
        Label star = new Label
        {
            Text = "★",
            FontSize = 14,
            TextColor = AppColors.Text,
            VerticalOptions = LayoutOptions.Center,
        }
        .Bind(IsVisibleProperty, nameof(PlaceSearchResultItem.IsFavorite));

        Label placeLabel = new Label
        {
            FontSize = 13,
            TextColor = AppColors.Text,
            VerticalOptions = LayoutOptions.Center,
        }
        .Bind(Label.TextProperty, nameof(PlaceSearchResultItem.DisplayText));

        // remove button, only shown on favorite rows. Tapping it asks for confirmation and removes
        // the favorite. It is a Button (not a tap gesture) so its tap is consumed and does not also
        // trigger the row's select gesture.
        Button removeButton = new Button
        {
            Text = "✕",
            FontSize = 13,
            TextColor = AppColors.TextMuted,
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            Padding = new Thickness(6, 0),
            HeightRequest = 34,
            WidthRequest = 40,
            VerticalOptions = LayoutOptions.Center,
        }
        .Bind(IsVisibleProperty, nameof(PlaceSearchResultItem.IsFavorite));
        removeButton.Command = new Command<PlaceSearchResultItem>(async item => await ConfirmRemoveFavorite(item));
        removeButton.SetBinding(Button.CommandParameterProperty, new Binding("."));

        Grid placeRow = new Grid
        {
            ColumnDefinitions = Columns.Define(GridLength.Auto, GridLength.Star, GridLength.Auto),
            ColumnSpacing = 9,
            Padding = new Thickness(14, 5, 4, 5),
        }
        .Bind<Grid, bool, bool>(
            IsVisibleProperty,
            nameof(PlaceSearchResultItem.IsSeparator),
            convert: isSeparator => !isSeparator);
        placeRow.Add(star, column: 0, row: 0);
        placeRow.Add(placeLabel, column: 1, row: 0);
        placeRow.Add(removeButton, column: 2, row: 0);

        // separator row between favorites and normal results
        Label separatorLabel = new Label
        {
            FontSize = 10,
            CharacterSpacing = 1.2,
            TextColor = AppColors.TextMuted,
            VerticalOptions = LayoutOptions.Center,
        }
        .Bind(Label.TextProperty, nameof(PlaceSearchResultItem.DisplayText));

        HorizontalStackLayout separatorRow = new HorizontalStackLayout
        {
            Spacing = 8,
            Padding = new Thickness(14, 6),
            BackgroundColor = AppColors.Background,
            Children = { separatorLabel }
        }
        .Bind(IsVisibleProperty, nameof(PlaceSearchResultItem.IsSeparator));

        // Opaque background so the whole row reliably receives touch on Android. A background-less
        // layout only gets touches where a child is actually drawn; without it neither the tap nor
        // the long-press fired, which made the rows completely unresponsive.
        var container = new Grid
        {
            BackgroundColor = AppColors.Card,
        };
        container.Add(placeRow);
        container.Add(separatorRow);

        // tap -> select the place (reliable gesture recognizer; parameter is the row's own item)
        var tap = new TapGestureRecognizer();
        tap.SetBinding(
            TapGestureRecognizer.CommandProperty,
            new Binding(nameof(MainPageViewModel.SelectPlaceCommand), source: _viewModel));
        tap.SetBinding(TapGestureRecognizer.CommandParameterProperty, new Binding("."));
        container.GestureRecognizers.Add(tap);

        return container;
    }

    /// <summary>Asks for confirmation and, if accepted, removes the given favorite row.</summary>
    private async Task ConfirmRemoveFavorite(PlaceSearchResultItem item)
    {
        // only favorite rows can be removed (not the separator or normal search results)
        if (item is null || !item.IsFavorite || item.Place is null)
        {
            return;
        }

        bool remove = await DisplayAlertAsync(
            AppResources.MainPage_RemoveFavoriteTitle,
            string.Format(AppResources.MainPage_RemoveFavoriteMessage, item.DisplayText),
            AppResources.Common_Yes,
            AppResources.Common_No);

        if (remove)
        {
            await _viewModel.RemoveFavorite(item.Place);
        }
    }
}
