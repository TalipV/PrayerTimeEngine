using PrayerTimeEngine.Presentation.GraphicsView;
using PrayerTimeEngine.Presentation.ViewModel;
using OnScreenSizeMarkup.Maui.Helpers;
using System.Reflection;
using Microsoft.Extensions.Logging;
using OnScreenSizeMarkup.Maui;

namespace PrayerTimeEngine
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;
        private readonly ILogger<MainPage> _logger;

        public MainPage(MainPageViewModel viewModel, ILogger<MainPage> logger)
        {
            InitializeComponent();
            _logger = logger;
            BindingContext = this._viewModel = viewModel;

            viewModel.OnAfterLoadingPrayerTimes_EventTrigger += ViewModel_OnAfterLoadingPrayerTimes_EventTrigger;
            viewModel.IsShakeEnabled = true;

            this.Loaded += MainPage_Loaded;
            this.searchBar.SearchButtonPressed += SearchBar_SearchButtonPressed;

            setCustomSizes();
        }

        private void setCustomSizes()
        {
            // Large: Galaxy S22 Ultra, iPhone 14 Pro Max
            // Medium: Google Pixel 5
            // ************************

#if DEBUG
            this.searchBar.Text = getScreenSizeCategoryName();
#endif

            // STATUS TEXTS
            new List<Label>()
            {
                lastUpdatedTextInfo, currentProfileLocationName
            }.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.OnScreenSize<double>(
                        defaultSize:99,
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
                        medium:22,
                        small: 99,
                        extraSmall:99);
            });

            // PRAYER TIME MAIN DURATIONS
            new List<Label>()
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

            //if (DeviceInfo.Platform == DevicePlatform.iOS)
            //    return;
            //else if (DeviceInfo.Platform == DevicePlatform.Android)
            //    return;
        }

        private string getScreenSizeCategoryName()
        {
            Type helpersType = typeof(OnScreenSizeHelpers);// Fetch screen diagonal from OnScreenSizeHelpers with no parameters
            MethodInfo getDiagonalMethod = helpersType?.GetMethod("GetScreenDiagonalInches",
                                                                 BindingFlags.Static | BindingFlags.NonPublic,
                                                                 null,
                                                                 new Type[] { },
                                                                 null);

            double screenDiagonalInches = (double)getDiagonalMethod?.Invoke(null, null);



            // 2. Fetch the Categorizer property from Manager.Current
            Type managerType = typeof(Manager);
            PropertyInfo instanceProperty = managerType?.GetProperty("Current", BindingFlags.Static | BindingFlags.Public);
            object managerInstance = instanceProperty?.GetValue(null);
            PropertyInfo categorizerProperty = managerType?.GetProperty("Categorizer", BindingFlags.Instance | BindingFlags.NonPublic);
            object categorizerInstance = categorizerProperty?.GetValue(managerInstance);

            // 3. Call GetCategoryByDiagonalSize method on Categorizer
            Type categorizerType = categorizerInstance.GetType();
            MethodInfo getCategoryMethod = categorizerType.GetMethod("GetCategoryByDiagonalSize");
            object[] parameters = new object[] { Manager.Current.Mappings, screenDiagonalInches };
            string result = getCategoryMethod?.Invoke(categorizerInstance, parameters)?.ToString() ?? "NOT FOUND";

            return result;
        }


        private void MainPage_Loaded(object sender, EventArgs e)
        {
            Task.Run(_viewModel.OnPageLoaded);
        }

        private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            this.Popup.IsOpen = true;
        }

        void searchResults_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            searchBar.Text = "";
            this.Popup.IsOpen = false;
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
