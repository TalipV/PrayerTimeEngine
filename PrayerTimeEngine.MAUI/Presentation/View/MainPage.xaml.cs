﻿using CommunityToolkit.Maui.Storage;
using OnScreenSizeMarkup.Maui.Helpers;
using PrayerTimeEngine.Presentation.GraphicsView;
using PrayerTimeEngine.Presentation.ViewModel;
using UraniumUI.Controls;
using UraniumUI.Material.Controls;

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

            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, EventArgs e)
        {
            // BUG crashes the app on startup
            return;

            var autoCompleteTextField = new AutoCompleteTextField
            {
                Title = "Search",
                BackgroundColor = Colors.DarkSlateGray,
            };

            autoCompleteTextField.SetBinding(AutoCompleteView.ItemsSourceProperty, nameof(_viewModel.FoundPlacesSelectionTexts));
            autoCompleteTextField.SetBinding(AutoCompleteView.TextProperty, nameof(_viewModel.PlaceSearchText));
            autoCompleteTextField.SetBinding(AutoCompleteView.SelectedTextProperty, nameof(_viewModel.SelectedPlaceText));

            this.autoCompleteTextFieldPlaceHolder.Content = autoCompleteTextField;
        }

        private async Task openOptionsMenu()
        {
            string action = await this.DisplayActionSheet(
                title: "Options",
                cancel: "Cancel",
                destruction: null,
                "Zeiten-Konfiguration",
                "Ortsdaten",
                "DB-Tabellen zeigen",
                "DB-File speichern",
                "Logs anzeigen");

            switch (action)
            {
                case "Zeiten-Konfiguration":
                    await displayAlert(this._viewModel.GetPrayerTimeConfigDisplayText());
                    break;
                case "Ortsdaten":
                    await displayAlert(this._viewModel.GetLocationDataDisplayText());
                    break;
                case "DB-Tabellen zeigen":
                    await this._viewModel.ShowDatabaseTable();
                    break;
                case "DB-File speichern":
                    FolderPickerResult folderPickerResult = await FolderPicker.PickAsync(CancellationToken.None);

                    if (folderPickerResult.Folder != null)
                    {
                        File.Copy(
                            sourceFileName: Core.Common.AppConfig.DATABASE_PATH,
                            destFileName: Path.Combine(folderPickerResult.Folder.Path, $"dbFile_{DateTime.Now:ddMMyyyy_HH_mm}.db"),
                            overwrite: true);
                    }

                    break;
                case "Logs anzeigen":
                    this._viewModel.GoToLogsPageCommand.Execute(null);
                    break;
                case "Cancel":
                    break;
            }

        }

        private async Task displayAlert(string text)
        {
            await DisplayAlert("Info", text, "Ok");
        }

        private void setCustomSizes()
        {
#if WINDOWS
            return;
#endif
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
