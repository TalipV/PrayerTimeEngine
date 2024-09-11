using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.Logging;
using NodaTime;
using OnScreenSizeMarkup.Maui.Helpers;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Services;
using PrayerTimeEngine.Presentation.Views;
using PrayerTimeEngine.Presentation.Views.MosquePrayerTimes;
using PrayerTimeEngine.Presentation.Views.PrayerTimes;
using UraniumUI.Material.Controls;

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
            ToastMessageService toastMessageService,
            IPreferenceService preferenceService,
            ISystemInfoService systemInfoService
        )
    {
        BindingContext = _viewModel = viewModel;
        _dispatcher = dispatcher;
        _preferenceService = preferenceService;
        _systemInfoService = systemInfoService;
        _mainPageOptionsMenuService =
            new MainPageOptionsMenuService(
                this,
                viewModel,
                toastMessageService,
                MauiProgram.ServiceProvider.GetRequiredService<ILogger<MainPageOptionsMenuService>>(),
                preferenceService);

        if (_preferenceService.CheckAndResetDoReset())
        {
            File.Delete(AppConfig.DATABASE_PATH);
        }

        Content = createUI();

        BackgroundColor = Color.FromRgb(243, 234, 227);

        viewModel.OnAfterLoadingPrayerTimes_EventTrigger += ViewModel_OnAfterLoadingPrayerTimes_EventTrigger;

        _lastUpdatedTextInfo.GestureRecognizers
            .Add(
                new TapGestureRecognizer
                {
                    Command = new Command(async () => await _mainPageOptionsMenuService.OpenGeneralOptionsMenu().ConfigureAwait(false)),
                    NumberOfTapsRequired = 2,
                });

        _profileDisplayNameTextInfo.GestureRecognizers
            .Add(
                new TapGestureRecognizer
                {
                    Command = new Command(async () => await _mainPageOptionsMenuService.OpenProfileOptionsMenu().ConfigureAwait(false)),
                    NumberOfTapsRequired = 2,
                });
    }

    private void ViewModel_OnAfterLoadingPrayerTimes_EventTrigger()
    {
        _dispatcher.Dispatch(() =>
        {
            ZonedDateTime zonedDateTime = _systemInfoService.GetCurrentZonedDateTime();
            prayerTimeGraphicView.DisplayPrayerTime = _viewModel.CurrentProfileWithModel.GetDisplayPrayerTime(zonedDateTime);
            prayerTimeGraphicViewBaseView.Invalidate();
        });
    }

    /// <summary>
    /// Triggers when the app is opened after being minimized
    /// </summary>
    private void app_Resumed()
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
            app.Resumed -= app_Resumed;
            app.Resumed += app_Resumed;
        }

        Task.Run(_viewModel.OnActualAppearing);
    }

    protected override void OnDisappearing()
    {
        if (Application.Current is App app)
        {
            app.Resumed -= app_Resumed;
        }
    }

    private Label _lastUpdatedTextInfo;
    private Label _profileDisplayNameTextInfo;
    private PrayerTimeGraphicView prayerTimeGraphicView;
    private GraphicsView prayerTimeGraphicViewBaseView;
    private CarouselView _carouselView;

    private Grid createUI()
    {
        var titleGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            }
        };

        _lastUpdatedTextInfo = new Label()
            .Column(0).Row(0)
            .Start()
            .CenterVertical()
            .Bind(
                Label.TextProperty,
                path: $"{nameof(MainPageViewModel.CurrentProfileWithModel)}.{nameof(IPrayerTimeViewModel.PrayerTimesSet)}.{nameof(DynamicPrayerTimesSet.DataCalculationTimestamp)}",
                stringFormat: "{0:dd.MM, HH:mm:ss}");

        titleGrid.Add(_lastUpdatedTextInfo);

        _profileDisplayNameTextInfo = new Label()
            .Column(1).Row(0)
            .Paddings(0, 0, 20, 0)
            .TextEnd()
            .CenterVertical()
            .Bind(Label.TextProperty, $"{nameof(MainPageViewModel.CurrentProfile)}.{nameof(Profile.Name)}");

        titleGrid.Add(_profileDisplayNameTextInfo);

        NavigationPage.SetTitleView(this, titleGrid);

        var searchBox = 
            new AutoCompleteTextField
            {
                Title = "Search",
                VerticalOptions = LayoutOptions.Center,
                BorderColor = Colors.Black,
                InputBackgroundColor = Colors.LightGray,
                TextColor = Colors.Black,
                TitleColor = Colors.Black,
            }
            .Bind(AutoCompleteTextField.ItemsSourceProperty, nameof(MainPageViewModel.FoundPlacesSelectionTexts))
            .Bind(AutoCompleteTextField.SelectedTextProperty, nameof(MainPageViewModel.SelectedPlaceText))
            .Bind(AutoCompleteTextField.TextProperty, nameof(MainPageViewModel.PlaceSearchText))
            .Bind<AutoCompleteTextField, bool, bool>(
                IsEnabledProperty,
                nameof(MainPageViewModel.IsLoadingPrayerTimesOrSelectedPlace),
                convert: value => !value);

        var prayerTimesGridView = new DynamicPrayerTimeView(_viewModel);

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(11, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(7, GridUnitType.Star) },
            }
        }
        .Paddings(10, 20, 10, 20);

        // Graphics View
        prayerTimeGraphicView = new PrayerTimeGraphicView();
        prayerTimeGraphicViewBaseView = new GraphicsView
        {
            Drawable = prayerTimeGraphicView
        }
        .Bind(OpacityProperty, nameof(MainPageViewModel.LoadingStatusOpacityValue));

        _carouselView = new CarouselView
        {
            ItemTemplate = new ProfileDataTemplateSelector
            {
                MosquePrayerTimeTemplate = new DataTemplate(() => new MosquePrayerTimeView()),
                PrayerTimesTemplate = new DataTemplate(() => new DynamicPrayerTimeView(_viewModel))
            },
            Loop = false
        }
        .FillHorizontal()
        .FillVertical()
        .Bind(ItemsView.ItemsSourceProperty, nameof(MainPageViewModel.ProfilesWithModel), mode: BindingMode.TwoWay)
        .Bind(CarouselView.CurrentItemProperty, nameof(MainPageViewModel.CurrentProfileWithModel), mode: BindingMode.TwoWay);

        mainGrid.AddWithSpan(searchBox, row: 0, column: 0);
        mainGrid.AddWithSpan(_carouselView, row: 1, column: 0);
        mainGrid.AddWithSpan(prayerTimeGraphicViewBaseView, row: 2, column: 0);

        mainGrid.SetBinding(OpacityProperty, new Binding(nameof(MainPageViewModel.LoadingStatusOpacityValue)));

        if (OperatingSystem.IsWindows())
        {
            return mainGrid;
        }

        // Large: Galaxy S22 Ultra, iPhone 14 Pro Max
        // Medium: Google Pixel 5
        // ************************

        // STATUS TEXTS
        new List<Label>()
        {
            _lastUpdatedTextInfo, _profileDisplayNameTextInfo
        }.ForEach(label =>
        {
            label.FontSize =
                OnScreenSizeHelpers.Instance.GetScreenSizeValue<double>(
                    defaultSize: DebugUtil.GetSizeValues(99)[0],
                    extraLarge: DebugUtil.GetSizeValues(99)[0],
                    large: DebugUtil.GetSizeValues(25)[0],
                    medium: DebugUtil.GetSizeValues(23)[0],
                    small: DebugUtil.GetSizeValues(99)[0],
                    extraSmall: DebugUtil.GetSizeValues(99)[0]);
        });

        return mainGrid;
    }
}
