using Microsoft.Extensions.Logging;
using OnScreenSizeMarkup.Maui.Helpers;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;
using PrayerTimeEngine.Presentation.Services;
using PrayerTimeEngine.Presentation.Views;
using PrayerTimeEngine.Presentation.Views.MosquePrayerTime;
using PrayerTimeEngine.Presentation.Views.PrayerTimes;
using UraniumUI.Material.Controls;

namespace PrayerTimeEngine.Presentation.Pages.Main
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;
        private readonly IDispatcher _dispatcher;
        private readonly ToastMessageService _toastMessageService;
        private readonly IPreferenceService _preferenceService;
        private readonly ILogger<MainPage> _logger;
        private readonly MainPageOptionsMenuService _mainPageOptionsMenuService;

        public MainPage(
                IDispatcher dispatcher,
                MainPageViewModel viewModel,
                ToastMessageService toastMessageService,
                IPreferenceService preferenceService,
                ILogger<MainPage> logger
            )
        {
            BindingContext = _viewModel = viewModel;
            _dispatcher = dispatcher;
            _toastMessageService = toastMessageService;
            _preferenceService = preferenceService;
            _logger = logger;
            _mainPageOptionsMenuService =
                new MainPageOptionsMenuService(
                    toastMessageService,
                    this,
                    viewModel,
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
                        Command = new Command(async () => await _mainPageOptionsMenuService.openOptionsMenu().ConfigureAwait(false)),
                        NumberOfTapsRequired = 2,
                    });

            _profileDisplayNameTextInfo.GestureRecognizers
                .Add(
                    new TapGestureRecognizer
                    {
                        Command = new Command(async () =>
                        {
                            switch (await DisplayActionSheet(
                                title: "Profilverwaltung",
                                cancel: "Abbrechen",
                                destruction: null,
                                "Neues Profil erstellen",
                                "Profil löschen"))
                            {
                                case "Neues Profil erstellen":
                                    await _viewModel.CreateNewProfile();
                                    break;
                                case "Profil löschen":
                                    await _viewModel.DeleteCurrentProfile();
                                    break;
                            }
                        }),
                        NumberOfTapsRequired = 2,
                    });
        }

        private void ViewModel_OnAfterLoadingPrayerTimes_EventTrigger()
        {
            _dispatcher.Dispatch(() =>
            {
                prayerTimeGraphicView.DisplayPrayerTime = _viewModel.GetDisplayPrayerTime();
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

        private Grid createUI()
        {
            // NavigationPage TitleView
            var titleGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };

            _lastUpdatedTextInfo = new Label
            {
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Center
            };
            _lastUpdatedTextInfo.SetBinding(Label.TextProperty, new Binding($"{nameof(MainPageViewModel.CurrentProfileWithModel)}.{nameof(PrayerTimeViewModel.PrayerTimeBundle)}.{nameof(PrayerTimesBundle.DataCalculationTimestamp)}", stringFormat: "{0:dd.MM, HH:mm:ss}"));
            titleGrid.AddWithSpan(_lastUpdatedTextInfo, row: 0, column: 0);

            _profileDisplayNameTextInfo = new Label
            {
                Padding = new Thickness(0, 0, 20, 0),
                HorizontalTextAlignment = TextAlignment.End,
                VerticalTextAlignment = TextAlignment.Center
            };
            _profileDisplayNameTextInfo.SetBinding(Label.TextProperty, nameof(MainPageViewModel.ProfileDisplayText));
            titleGrid.AddWithSpan(_profileDisplayNameTextInfo, row: 0, column: 1);

            NavigationPage.SetTitleView(this, titleGrid);

            var searchBox = new AutoCompleteTextField
            {
                Title = "Search",
                VerticalOptions = LayoutOptions.Center,
                BorderColor = Colors.Black,
                InputBackgroundColor = Colors.LightGray,
                TextColor = Colors.Black,
                TitleColor = Colors.Black,
            };
            searchBox.SetBinding(AutoCompleteTextField.ItemsSourceProperty, nameof(MainPageViewModel.FoundPlacesSelectionTexts));
            searchBox.SetBinding(AutoCompleteTextField.SelectedTextProperty, nameof(MainPageViewModel.SelectedPlaceText));
            searchBox.SetBinding(AutoCompleteTextField.TextProperty, nameof(MainPageViewModel.PlaceSearchText));
            searchBox.SetBinding(IsEnabledProperty, nameof(MainPageViewModel.IsNotLoadingPrayerTimesOrSelectedPlace));

            var prayerTimesGridView = new PrayerTimesView(_viewModel);

            var mainGrid = new Grid
            {
                Padding = new Thickness(10, 20, 10, 20),
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(11, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(7, GridUnitType.Star) },
                }
            };

            // Graphics View
            prayerTimeGraphicView = new PrayerTimeGraphicView();
            prayerTimeGraphicViewBaseView = new GraphicsView
            {
                Drawable = prayerTimeGraphicView
            };
            prayerTimeGraphicViewBaseView.SetBinding(OpacityProperty, new Binding(nameof(MainPageViewModel.LoadingStatusOpacityValue)));

            var carouselView = new CarouselView
            {
                ItemTemplate = new DataTemplate(() =>
                {
                    var prayerTimeView = new PrayerTimesView(_viewModel);
                    return prayerTimeView;
                }),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Loop = false
            };

            // Bind the SelectedItem to CurrentProfile
            carouselView.SetBinding(ItemsView.ItemsSourceProperty, new Binding(nameof(MainPageViewModel.ProfilesWithModel), mode: BindingMode.TwoWay));
            carouselView.SetBinding(CarouselView.CurrentItemProperty, new Binding(nameof(MainPageViewModel.CurrentProfileWithModel), mode: BindingMode.TwoWay));

            mainGrid.AddWithSpan(searchBox, row: 0, column: 0);
            mainGrid.AddWithSpan(carouselView, row: 1, column: 0);
            mainGrid.AddWithSpan(prayerTimeGraphicViewBaseView, row: 2, column: 0);

            mainGrid.SetBinding(OpacityProperty, new Binding(nameof(MainPageViewModel.LoadingStatusOpacityValue)));

            if (OperatingSystem.IsWindows())
            {
                return mainGrid;
            }

            // Large: Galaxy S22 Ultra, iPhone 14 Pro Max
            // Medium: Google Pixel 5
            // ************************

#if DEBUG
            //this.searchBar.Text = DebugUtil.GetScreenSizeCategoryName();
#endif

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
}
