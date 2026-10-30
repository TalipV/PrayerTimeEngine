using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.Shapes;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Views.MosquePrayerTimes;
using PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic;
using PrayerTimeEngine.Presentation.Views.PrayerTimes;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

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

        _profileDisplayNameTextInfo.GestureRecognizers
            .Add(
                new TapGestureRecognizer
                {
                    Command = new Command(async () => await _mainPageOptionsMenuService.OpenOptionsMenu().ConfigureAwait(false)),
                    NumberOfTapsRequired = 2,
                });
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
                GridLength.Star)
        };

        _profileDisplayNameTextInfo = new Label()
            .Column(0).Row(0).Paddings(0, 0, 20, 0)
            .Start().CenterVertical()
            .MinWidth(20) // otherwise we can't reach the menu by tapping an empty label
            .FontSize(AppFontSizes.Title)
            .TextColor(AppColors.Text)
            .Bind(Label.TextProperty, $"{nameof(MainPageViewModel.CurrentProfile)}.{nameof(Profile.Name)}");
        _titleGrid.Add(_profileDisplayNameTextInfo);

        _weeksUntilTextInfo = new Label()
            .Column(1).Row(0).Paddings(0, 0, 20, 0)
            .End().CenterVertical()
            .FontSize(AppFontSizes.Subtitle)
            .TextColor(AppColors.Text)
            .Bind(Label.TextProperty, $"{nameof(MainPageViewModel.WeeksUntilLabelText)}");
        _titleGrid.Add(_weeksUntilTextInfo);

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
        }
        .Bind(OpacityProperty, nameof(MainPageViewModel.LoadingStatusOpacityValue));

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

        mainGrid.SetBinding(OpacityProperty, new Binding(nameof(MainPageViewModel.LoadingStatusOpacityValue)));

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

        return mainGrid;
    }

    /// <summary>The search input field. Bound to the search text; opens/closes the dropdown on focus change.</summary>
    private Border CreateSearchField()
    {
        Entry searchEntry = new Entry
        {
            Placeholder = "Search",
            BackgroundColor = Colors.Transparent,
            TextColor = AppColors.Text,
            PlaceholderColor = AppColors.Text,
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

        return new Border
        {
            BackgroundColor = AppColors.Surface,
            Stroke = AppColors.Border,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(8, 0),
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
            BackgroundColor = Color.FromArgb("#132038"),
            Stroke = AppColors.Border,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
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
            TextColor = AppColors.Accent,
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
            TextColor = Color.FromArgb("#7F8EA8"),
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
            TextColor = Color.FromArgb("#7F8EA8"),
            VerticalOptions = LayoutOptions.Center,
        }
        .Bind(Label.TextProperty, nameof(PlaceSearchResultItem.DisplayText));

        HorizontalStackLayout separatorRow = new HorizontalStackLayout
        {
            Spacing = 8,
            Padding = new Thickness(14, 6),
            BackgroundColor = Color.FromArgb("#0F1B30"),
            Children = { separatorLabel }
        }
        .Bind(IsVisibleProperty, nameof(PlaceSearchResultItem.IsSeparator));

        // Opaque background so the whole row reliably receives touch on Android. A background-less
        // layout only gets touches where a child is actually drawn; without it neither the tap nor
        // the long-press fired, which made the rows completely unresponsive.
        var container = new Grid
        {
            BackgroundColor = Color.FromArgb("#132038"),
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
            "Favorit entfernen?",
            $"„{item.DisplayText}“ aus den Favoriten entfernen?",
            "Ja",
            "Nein");

        if (remove)
        {
            await _viewModel.RemoveFavorite(item.Place);
        }
    }
}
