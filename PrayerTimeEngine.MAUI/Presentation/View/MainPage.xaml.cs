using CommunityToolkit.Maui.Storage;
using OnScreenSizeMarkup.Maui.Helpers;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Presentation.GraphicsView;
using PrayerTimeEngine.Presentation.ViewModel;

namespace PrayerTimeEngine
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;

        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = this._viewModel = viewModel;

            viewModel.OnAfterLoadingPrayerTimes_EventTrigger += ViewModel_OnAfterLoadingPrayerTimes_EventTrigger;

            setCustomSizes();

            this.lastUpdatedTextInfo.GestureRecognizers
                .Add(
                    new TapGestureRecognizer
                    {
                        Command = new Command(async () => await this.openOptionsMenu()),
                        NumberOfTapsRequired = 2,
                    });
        }

        private const string _optionsText = "Optionen";
        private const string _showTimeConfigsOverviewText = "Überblick: Zeiten-Konfiguration";
        private const string _showLocationConfigsOverviewText = "Überblick: Ortsdaten";
        private const string _showLogsText = "Logs anzeigen";
        private const string _showDbTablesText = "DB-Tabellen anzeigen";
        private const string _saveDbFileText = "DB-Datei speichern";
        private const string _resetAppText = "App-Daten zurücksetzen";
        private const string _cancelText = "Abbrechen";

        private async Task openOptionsMenu()
        {
            string action = await this.DisplayActionSheet(
                title: _optionsText,
                cancel: "Abbrechen",
                destruction: null,
                _showTimeConfigsOverviewText,
                _showLocationConfigsOverviewText,
                _showLogsText,
                _showDbTablesText,
                _saveDbFileText,
                _resetAppText);

            switch (action)
            {
                case _showTimeConfigsOverviewText:
                    await DisplayAlert("Info", this._viewModel.GetPrayerTimeConfigDisplayText(), "Ok");
                    break;
                case _showLocationConfigsOverviewText:
                    await DisplayAlert("Info", this._viewModel.GetLocationDataDisplayText(), "Ok");
                    break;
                case _showLogsText:
                    this._viewModel.GoToLogsPageCommand.Execute(null);
                    break;
                case _showDbTablesText:
                    await this._viewModel.ShowDatabaseTable();
                    break;
                case _saveDbFileText:
                    FolderPickerResult folderPickerResult = await FolderPicker.PickAsync(CancellationToken.None);

                    if (folderPickerResult.Folder != null)
                    {
                        File.Copy(
                            sourceFileName: Core.Common.AppConfig.DATABASE_PATH,
                            destFileName: Path.Combine(folderPickerResult.Folder.Path, $"dbFile_{DateTime.Now:ddMMyyyy_HH_mm}.db"),
                            overwrite: true);
                    }

                    break;
                case _resetAppText:
                    if (!await DisplayAlert("Bestätigung", "Daten wirklich zurücksetzen?", "Ja", "Abbrechen"))
                        break;

                    if (File.Exists(AppConfig.DATABASE_PATH))
                        File.Delete(AppConfig.DATABASE_PATH);
                    Application.Current.Quit();
                    break;
                case _cancelText:
                    break;
            }

        }

        private void setCustomSizes()
        {
            if (OperatingSystem.IsWindows())
            {
                return;
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
                lastUpdatedTextInfo, currentProfileLocationName
            }.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.Instance.GetScreenSizeValue<double>(
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
                    OnScreenSizeHelpers.Instance.GetScreenSizeValue<double>(
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
                    OnScreenSizeHelpers.Instance.GetScreenSizeValue<double>(
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
                    OnScreenSizeHelpers.Instance.GetScreenSizeValue<double>(
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
                    OnScreenSizeHelpers.Instance.GetScreenSizeValue<double>(
                        defaultSize: 99,
                        extraLarge: 99,
                        large: 14,
                        medium: 11,
                        small: 99,
                        extraSmall: 99);
            });
        }

        private void ViewModel_OnAfterLoadingPrayerTimes_EventTrigger()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PrayerTimeGraphicView.DisplayPrayerTime = _viewModel.GetDisplayPrayerTime();
                PrayerTimeGraphicViewBase.Invalidate();
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
    }
}
