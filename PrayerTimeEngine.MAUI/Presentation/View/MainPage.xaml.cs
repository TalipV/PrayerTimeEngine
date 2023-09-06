using PrayerTimeEngine.Presentation.GraphicsView;
using PrayerTimeEngine.Presentation.ViewModel;
using OnScreenSizeMarkup.Maui.Helpers;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Presentation;
using DevExpress.Maui.Editors;
using PrayerTimeEngine.Core.Domain.PlacesService.Models.Common;

namespace PrayerTimeEngine
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;
        private readonly ILogger<MainPage> _logger;

        public MainPage(MainPageViewModel viewModel, ILogger<MainPage> logger)
        {
            //var talip = this.LoadFromXaml(typeof(MainPage));

            InitializeComponent();

            _logger = logger;
            BindingContext = this._viewModel = viewModel;

            viewModel.OnAfterLoadingPrayerTimes_EventTrigger += ViewModel_OnAfterLoadingPrayerTimes_EventTrigger;
            viewModel.IsShakeEnabled = true;

            this.Loaded += MainPage_Loaded;

            setCustomSizes();

            var asyncItemsSourceProvider = new AsyncItemsSourceProvider
            {
                RequestDelay = 1000,
                CharacterCountThreshold = 4
            };

            asyncItemsSourceProvider.ItemsRequested += AsyncItemsSourceProvider_ItemsRequested;
            autoCompleteSearch.ItemsSourceProvider = asyncItemsSourceProvider;

            this.lastUpdatedTextInfo.GestureRecognizers
                .Add(
                    new TapGestureRecognizer
                    {
                        Command = new Command(async () => await displayAlert(this._viewModel.GetPrayerTimeConfigDisplayText())),
                        NumberOfTapsRequired = 2,
                    });

            this.currentProfileLocationName.GestureRecognizers
                .Add(
                    new TapGestureRecognizer
                    {
                        Command = new Command(async () => await displayAlert(this._viewModel.GetLocationDataDisplayText())),
                        NumberOfTapsRequired = 2,
                    });
        }

        private async Task displayAlert(string text)
        {
            await DisplayAlert("Info", text, "Ok");
        }

        private void AsyncItemsSourceProvider_ItemsRequested(object sender, ItemsRequestEventArgs e)
        {
            Func<System.Collections.IEnumerable> func = 
                () => {
                    var placeSearchTask = Task.Run(async () => await this._viewModel.PerformPlaceSearch(e.Text));
                    placeSearchTask.Wait();
                    return placeSearchTask.Result;
                };

            Func<Task<System.Collections.IEnumerable>> funcx =
                async () =>
                {
                    return await this._viewModel.PerformPlaceSearch(e.Text);
                };
        }

        private void setCustomSizes()
        {
            // Large: Galaxy S22 Ultra, iPhone 14 Pro Max
            // Medium: Google Pixel 5
            // ************************

#if DEBUG
            //this.searchBar.Text = DebugUtil.GetScreenSizeCategoryName();
#endif

            // STATUS TEXTS
            new List<Label>()
            {
                lastUpdatedTextInfo, currentProfileLocationName
            }.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.OnScreenSize<double>(
                        defaultSize: 99,
                        extraLarge: 99,
                        large: 25,
                        medium: 23,
                        small: 99,
                        extraSmall: 99);
            });

            // PRAYER TIME MAIN TITLES
            new List<Label>()
            {
                FajrName, DuhaName, DhuhrName, AsrName,
                MaghribName, IshaName
            }.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.OnScreenSize<double>(
                        defaultSize: 99,
                        extraLarge: 99,
                        large: 24,
                        medium: 22,
                        small: 99,
                        extraSmall: 99);
            });

            // PRAYER TIME MAIN DURATIONS
            new List<Label>
            {
                FajrDurationText, DuhaDurationText, DhuhrDurationText, AsrDurationText,
                MaghribDurationText, IshaDurationText
            }.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.OnScreenSize<double>(
                        defaultSize: 99,
                        extraLarge: 99,
                        large: 18,
                        medium: 14,
                        small: 99,
                        extraSmall: 99);
            });

            // PRAYER TIME SUB TIME NAMES
            new List<Label>()
            {
                FajrSubtimeGhalasName, FajrSubtimeRednessName,
                DuhaSubtimeQuarterName,
                AsrSubtimeMithlaynName, AsrSubtimeKarahaName,
                MaghribSubtimeSufficientName, MaghribSubtimeIshtibaqName,
                IshaSubtimeOneThirdName, IshaSubtimeOneHalfName, IshaSubtimeTwoThirdsName
            }.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.OnScreenSize<double>(
                        defaultSize: 99,
                        extraLarge: 99,
                        large: 14,
                        medium: 12,
                        small: 99,
                        extraSmall: 99);
            });

            // PRAYER TIME SUB TIMES 
            new List<Label>()
            {
                FajrSubtimeGhalasDisplayText, FajrSubtimeRednessDisplayText,
                DuhaSubtimeQuarterDisplayText,
                AsrSubtimeMithlaynDisplayText, AsrSubtimeKarahaDisplayText,
                MaghribSubtimeSufficientDisplayText, MaghribSubtimeIshtibaqDisplayText,
                IshaSubtimeOneThirdDisplayText, IshaSubtimeOneHalfDisplayText, IshaSubtimeTwoThirdsDisplayText
            }.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.OnScreenSize<double>(
                        defaultSize: 99,
                        extraLarge: 99,
                        large: 14,
                        medium: 11,
                        small: 99,
                        extraSmall: 99);
            });
        }

        private void MainPage_Loaded(object sender, EventArgs e)
        {
            Task.Run(_viewModel.OnPageLoaded);
        }

        private void ViewModel_OnAfterLoadingPrayerTimes_EventTrigger()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PrayerTimeGraphicView.DisplayPrayerTime = _viewModel.DisplayPrayerTime;
                PrayerTimeGraphicViewBase.Invalidate();
            });
        }

        /// <summary>
        /// Triggers when the app is opened after being minimized
        /// </summary>
        private void app_Resumed()
        {
            _viewModel.OnActualAppearing();
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

            _viewModel.OnActualAppearing();
        }

        protected override void OnDisappearing()
        {
            if (Application.Current is App app)
            {
                app.Resumed -= app_Resumed;
            }
        }
    }
}
