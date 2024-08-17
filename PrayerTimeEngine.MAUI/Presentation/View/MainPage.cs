using CommunityToolkit.Maui.Storage;
using OnScreenSizeMarkup.Maui.Helpers;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Presentation.GraphicsViews;
using PrayerTimeEngine.Presentation.ViewModel;
using UraniumUI.Material.Controls;

namespace PrayerTimeEngine.Presentation.View
{
    public partial class MainPage : ContentPage
    {
        private readonly IDispatcher _dispatcher;
        private readonly MainPageViewModel _viewModel;

        public MainPage(IDispatcher dispatcher, MainPageViewModel viewModel)
        {
            _dispatcher = dispatcher;
            BindingContext = _viewModel = viewModel;
            Content = createUI();

            BackgroundColor = Color.FromRgb(243, 234, 227);

            viewModel.OnAfterLoadingPrayerTimes_EventTrigger += ViewModel_OnAfterLoadingPrayerTimes_EventTrigger;

            lastUpdatedTextInfo.GestureRecognizers
                .Add(
                    new TapGestureRecognizer
                    {
                        Command = new Command(async () => await openOptionsMenu().ConfigureAwait(false)),
                        NumberOfTapsRequired = 2,
                    });
        }

        private const string _optionsText = "Optionen";

        private const string _generalOptionText = "... Allgemeines";
        private const string _showTimeConfigsOverviewText = "Überblick: Zeiten-Konfiguration";
        private const string _showLocationConfigsOverviewText = "Überblick: Ortsdaten";
        private const string _showLogsText = "Logs anzeigen";
        private const string _setCustomTextSizes = "Benutzerdefinierte Textgröße";

        private const string _technicalOptionText = "... Technisches";
        private const string _showDbTablesText = "DB-Tabellen anzeigen";
        private const string _saveDbFileText = "DB-Datei speichern";
        private const string _deviceInfoText = "Geräte-Informationen";

        private const string _systemOptionText = "... System";
        private const string _resetAppText = "App-Daten zurücksetzen";
        private const string _closeAppText = "App schließen";

        private const string _backText = "Zurück";
        private const string _cancelText = "Abbrechen";

        private async Task openOptionsMenu()
        {
            bool doRepeat;

            do
            {
                doRepeat = false;

                switch (await DisplayActionSheet(
                    title: _optionsText,
                    cancel: _cancelText,
                    destruction: null,
                    _generalOptionText,
                    _technicalOptionText,
                    _systemOptionText))
                {
                    case _generalOptionText:

                        switch (await DisplayActionSheet(
                            title: _generalOptionText,
                            cancel: _backText,
                            destruction: null,
                            _showTimeConfigsOverviewText,
                            _showLocationConfigsOverviewText,
                            _showLogsText,
                            _setCustomTextSizes))
                        {
                            case _showTimeConfigsOverviewText:
                                await DisplayAlert("Info", _viewModel.GetPrayerTimeConfigDisplayText(), "Ok");
                                break;
                            case _showLocationConfigsOverviewText:
                                await DisplayAlert("Info", _viewModel.GetLocationDataDisplayText(), "Ok");
                                break;
                            case _showLogsText:
                                _viewModel.GoToLogsPageCommand.Execute(null);
                                break;
                            case _setCustomTextSizes:
                                showCustomTextSizesInputPopup();
                                break;
                            case _backText:
                                doRepeat = true;
                                break;
                        }

                        break;

                    case _technicalOptionText:

                        switch (await DisplayActionSheet(
                            title: _technicalOptionText,
                            cancel: _backText,
                            destruction: null,
                            _showDbTablesText,
                            _saveDbFileText,
                            _deviceInfoText))
                        {
                            case _showDbTablesText:
                                await _viewModel.ShowDatabaseTable();
                                break;
                            case _saveDbFileText:
                                FolderPickerResult folderPickerResult = await FolderPicker.PickAsync(CancellationToken.None);

                                if (folderPickerResult.Folder != null)
                                {
                                    File.Copy(
                                        sourceFileName: AppConfig.DATABASE_PATH,
                                        destFileName: Path.Combine(folderPickerResult.Folder.Path, $"dbFile_{DateTime.Now:ddMMyyyy_HH_mm}.db"),
                                        overwrite: true);
                                }

                                break;

                            case _deviceInfoText:
                                await DisplayAlert(
                                    "Geräteinformationen",
                                    $"""
                                        Modell: {DeviceInfo.Manufacturer.ToUpper()}, {DeviceInfo.Model}
                                        Art: {DeviceInfo.Idiom}, {DeviceInfo.DeviceType}
                                        OS: {DeviceInfo.Platform}, {DeviceInfo.VersionString}
                                        Auflösung: {DeviceDisplay.MainDisplayInfo.Height}x{DeviceDisplay.MainDisplayInfo.Width} (Dichte: {DeviceDisplay.MainDisplayInfo.Density})
                                        Kategorie der Größe: {DebugUtil.GetScreenSizeCategoryName()}
                                    """
                                    , "Ok");
                                break;
                            case _backText:
                                doRepeat = true;
                                break;
                        }

                        break;
                    case _systemOptionText:

                        switch (await DisplayActionSheet(
                            title: _systemOptionText,
                            cancel: _backText,
                            destruction: null,
                            _resetAppText,
                            _closeAppText))
                        {
                            case _resetAppText:
                                if (!await DisplayAlert("Bestätigung", "Daten wirklich zurücksetzen?", "Ja", _cancelText))
                                    break;

                                if (File.Exists(AppConfig.DATABASE_PATH))
                                    File.Delete(AppConfig.DATABASE_PATH);
                                Application.Current.Quit();
                                break;
                            case _closeAppText:
                                Application.Current.Quit();
                                break;
                            case _backText:
                                doRepeat = true;
                                break;
                        }

                        break;
                    case _cancelText:
                        break;
                }
            }
            while (doRepeat);
        }

        private async void showCustomTextSizesInputPopup()
        {
            var currentValues = DebugUtil.GetSizeValues(0);

            if (currentValues.All(x => x == 0))
            {
                currentValues = [25, 24, 18, 14, 14];
            }

            string initialValue = string.Join(",", currentValues);

            string result =
                await DisplayPromptAsync(
                "Fünf Textgröße angeben",
                """
                Geben Sie Werte für die folgenden vier Textarten komma-separiert an:
                1. Statustexte oben
                2. Gebetszeiten-Namen
                3. Haupt-Gebetszeiten
                4. Sub-Gebetszeiten-Namen
                5. Sub-Gebetszeiten

                Zum Zurücksetzen "0,0,0,0,0" eingeben und bestätigen.
                """,
                initialValue: initialValue,
                keyboard: Keyboard.Text) ?? "";

            int[] sizeValues =
                result.Split(",")
                .Select(x => x.Replace(" ", ""))
                .Where(x => int.TryParse(x, out int _))
                .Select(int.Parse)
                .ToArray();

            if (sizeValues.Length == 5 && sizeValues.All(x => x >= 0))
            {
                // Save the value
                DebugUtil.SetSizeValue(sizeValues);

                await DisplayAlert("Erfolg", $"Gespeichert! App manuell wieder starten!", "OK");
                Application.Current.Quit();
            }
            else
            {
                await DisplayAlert("Eingabe ungültig", "Eingabe war ungültig", "OK");
            }
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



        private Label lastUpdatedTextInfo;
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

            var activityIndicator = new ActivityIndicator
            {
                IsRunning = true,
            };
            activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(MainPageViewModel.IsLoadingPrayerTimesOrSelectedPlace));
            titleGrid.AddWithSpan(activityIndicator, row: 0, column: 0, columnSpan: 2);

            lastUpdatedTextInfo = new Label
            {
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Center
            };
            lastUpdatedTextInfo.SetBinding(Label.TextProperty, new Binding("PrayerTimeBundle.DataCalculationTimestamp", stringFormat: "{0:dd.MM, HH:mm:ss}"));
            lastUpdatedTextInfo.SetBinding(IsVisibleProperty, nameof(MainPageViewModel.IsNotLoadingPrayerTimesOrSelectedPlace));
            titleGrid.AddWithSpan(lastUpdatedTextInfo, row: 0, column: 0);

            var currentProfilePlaceName = new Label
            {
                Padding = new Thickness(0, 0, 20, 0),
                HorizontalTextAlignment = TextAlignment.End,
                VerticalTextAlignment = TextAlignment.Center
            };
            currentProfilePlaceName.SetBinding(Label.TextProperty, "CurrentProfile.PlaceInfo.City");
            currentProfilePlaceName.SetBinding(IsVisibleProperty, nameof(MainPageViewModel.IsNotLoadingPrayerTimesOrSelectedPlace));
            titleGrid.AddWithSpan(currentProfilePlaceName, row: 0, column: 1);

            NavigationPage.SetTitleView(this, titleGrid);

            // Main Grid
            var mainGrid = new Grid
            {
                Padding = new Thickness(10, 20, 10, 20),
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(3, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(3, GridUnitType.Star) },

                    new RowDefinition { Height = new GridLength(0) },

                    new RowDefinition { Height = new GridLength(0) },

                    // Fajr & Duha
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Star },

                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    
                    // Dhuhr & Asr
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Star },

                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    
                    // Maghrib & Isha
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Star },

                    new RowDefinition { Height = GridLength.Star },

                    new RowDefinition { Height = new GridLength(12, GridUnitType.Star) }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }
                }
            };

            var searchBox = new AutoCompleteTextField
            {
                Title = "Search",
                BackgroundColor = Colors.DarkSlateGray
            };
            searchBox.SetBinding(AutoCompleteTextField.ItemsSourceProperty, nameof(MainPageViewModel.FoundPlacesSelectionTexts));
            searchBox.SetBinding(AutoCompleteTextField.SelectedTextProperty, nameof(MainPageViewModel.SelectedPlaceText));
            searchBox.SetBinding(AutoCompleteTextField.TextProperty, nameof(MainPageViewModel.PlaceSearchText));
            searchBox.SetBinding(IsEnabledProperty, nameof(MainPageViewModel.IsNotLoadingSelectedPlace));
            mainGrid.AddWithSpan(searchBox, row: 0, column: 0, columnSpan: 5);

            // Add prayer time labels and text for each prayer time
            addPrayerTimeUI(mainGrid, "Fajr", durationBinding: "PrayerTimeBundle.Fajr.DurationDisplayText", startRowNo: 4, startColumnNo: 0,
                subtime1Name: "Ghalas", showSubtime1Binding: nameof(MainPageViewModel.ShowFajrGhalas), subtime1Binding: "PrayerTimeBundle.Fajr.Ghalas",
                subtime2Name: "Redness", showSubtime2Binding: nameof(MainPageViewModel.ShowFajrRedness), subtime2Binding: "PrayerTimeBundle.Fajr.Karaha");

            addPrayerTimeUI(mainGrid, "Duha", durationBinding: "PrayerTimeBundle.Duha.DurationDisplayText", startRowNo: 4, startColumnNo: 3,
                subtime1Name: "Quarter", subtime1Binding: "PrayerTimeBundle.Duha.QuarterOfDay");

            addPrayerTimeUI(mainGrid, "Dhuhr", durationBinding: "PrayerTimeBundle.Dhuhr.DurationDisplayText", startRowNo: 8, startColumnNo: 0);

            addPrayerTimeUI(mainGrid, "Asr", durationBinding: "PrayerTimeBundle.Asr.DurationDisplayText", startRowNo: 8, startColumnNo: 3,
                subtime1Name: "Mithlayn", showSubtime1Binding: nameof(MainPageViewModel.ShowMithlayn), subtime1Binding: "PrayerTimeBundle.Asr.Mithlayn",
                subtime2Name: "Karaha", showSubtime2Binding: nameof(MainPageViewModel.ShowKaraha), subtime2Binding: "PrayerTimeBundle.Asr.Karaha");

            addPrayerTimeUI(mainGrid, "Maghrib", durationBinding: "PrayerTimeBundle.Maghrib.DurationDisplayText", startRowNo: 12, startColumnNo: 0,
                subtime1Name: "Sufficient", showSubtime1Binding: nameof(MainPageViewModel.ShowMaghribSufficientTime), subtime1Binding: "PrayerTimeBundle.Maghrib.SufficientTime",
                subtime2Name: "Ishtibaq", showSubtime2Binding: nameof(MainPageViewModel.ShowIshtibaq), subtime2Binding: "PrayerTimeBundle.Maghrib.Ishtibaq");

            addPrayerTimeUI(mainGrid, "Isha", durationBinding: "PrayerTimeBundle.Isha.DurationDisplayText", startRowNo: 12, startColumnNo: 3,
                subtime1Name: "1/3", subtime1Binding: "PrayerTimeBundle.Isha.FirstThirdOfNight",
                subtime2Name: "1/2", subtime2Binding: "PrayerTimeBundle.Isha.MiddleOfNight",
                subtime3Name: "2/3", subtime3Binding: "PrayerTimeBundle.Isha.SecondThirdOfNight");

            // Graphics View
            prayerTimeGraphicView = new PrayerTimeGraphicView();
            prayerTimeGraphicViewBaseView = new GraphicsView
            {
                Drawable = prayerTimeGraphicView
            };
            mainGrid.AddWithSpan(prayerTimeGraphicViewBaseView, row: 17, column: 0, columnSpan: 5);

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
                lastUpdatedTextInfo, currentProfilePlaceName
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

        private void addPrayerTimeUI(
            Grid grid,
            string prayerName,
            string durationBinding,
            int startRowNo, int startColumnNo,
            string subtime1Name = null, string showSubtime1Binding = null, string subtime1Binding = null,
            string subtime2Name = null, string showSubtime2Binding = null, string subtime2Binding = null,
            string showSubtime3Binding = null, string subtime3Name = null, string subtime3Binding = null)
        {
            List<Label> timeTextViews = [];
            List<Label> timeDisplayTextViews = [];
            List<Label> subTimeTextViews = [];
            List<Label> subTimeDisplayTextViews = [];

            var prayerNameLabel = new Label
            {
                Text = prayerName,
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            };
            prayerNameLabel.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = _viewModel.GoToSettingsPageCommand,
                CommandParameter = Enum.Parse<EPrayerType>(prayerName)
            });
            grid.AddWithSpan(prayerNameLabel, startRowNo, startColumnNo, columnSpan: 2);

            var prayerDurationLabel = new Label
            {
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            };
            prayerDurationLabel.SetBinding(Label.TextProperty, new Binding(durationBinding, stringFormat: "{0:HH:mm:ss}"));
            grid.AddWithSpan(prayerDurationLabel, startRowNo + 1, startColumnNo, columnSpan: 2);

            timeTextViews.Add(prayerNameLabel);
            timeDisplayTextViews.Add(prayerDurationLabel);

            if (!string.IsNullOrEmpty(subtime1Binding))
            {
                var subtime1Label = new Label
                {
                    Text = subtime1Name,
                    TextColor = Colors.Black,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center
                };

                if (!string.IsNullOrEmpty(showSubtime1Binding))
                {
                    subtime1Label.SetBinding(IsVisibleProperty, new Binding(showSubtime1Binding, stringFormat: "{0:HH:mm:ss}"));
                }

                var subtime1DisplayText = new Label
                {
                    Text = subtime2Name,
                    TextColor = Colors.Black,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                subtime1DisplayText.SetBinding(Label.TextProperty, new Binding(subtime1Binding, stringFormat: "{0:HH:mm:ss}"));
                if (!string.IsNullOrEmpty(showSubtime1Binding))
                {
                    subtime1DisplayText.SetBinding(IsVisibleProperty, new Binding(showSubtime1Binding, stringFormat: "{0:HH:mm:ss}"));
                }

                grid.AddWithSpan(subtime1Label, startRowNo + 2, startColumnNo);
                grid.AddWithSpan(subtime1DisplayText, startRowNo + 2, startColumnNo + 1);

                subTimeTextViews.Add(subtime1Label);
                subTimeDisplayTextViews.Add(subtime1DisplayText);
            }

            if (!string.IsNullOrEmpty(subtime2Binding))
            {
                var subtime2Label = new Label
                {
                    Text = subtime2Name,
                    TextColor = Colors.Black,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start
                };
                if (!string.IsNullOrEmpty(showSubtime2Binding))
                {
                    subtime2Label.SetBinding(IsVisibleProperty, new Binding(showSubtime2Binding, stringFormat: "{0:HH:mm:ss}"));
                }

                var subtime2DisplayText = new Label
                {
                    TextColor = Colors.Black,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Start
                };

                subtime2DisplayText.SetBinding(Label.TextProperty, new Binding(subtime2Binding, stringFormat: "{0:HH:mm:ss}"));

                if (!string.IsNullOrEmpty(showSubtime2Binding))
                {
                    subtime2DisplayText.SetBinding(IsVisibleProperty, new Binding(showSubtime2Binding, stringFormat: "{0:HH:mm:ss}"));
                }

                grid.AddWithSpan(subtime2Label, startRowNo + 3, startColumnNo);
                grid.AddWithSpan(subtime2DisplayText, startRowNo + 3, startColumnNo + 1);

                subTimeTextViews.Add(subtime2Label);
                subTimeDisplayTextViews.Add(subtime2DisplayText);
            }

            if (!string.IsNullOrEmpty(subtime3Binding))
            {
                var subtime3Label = new Label
                {
                    Text = subtime3Name,
                    TextColor = Colors.Black,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start
                };
                if (!string.IsNullOrEmpty(showSubtime3Binding))
                {
                    subtime3Label.SetBinding(IsVisibleProperty, new Binding(showSubtime3Binding, stringFormat: "{0:HH:mm:ss}"));
                }

                var subtime3DisplayText = new Label
                {
                    TextColor = Colors.Black,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Start
                };

                subtime3DisplayText.SetBinding(Label.TextProperty, new Binding(subtime3Binding, stringFormat: "{0:HH:mm:ss}"));

                if (!string.IsNullOrEmpty(showSubtime3Binding))
                {
                    subtime3DisplayText.SetBinding(IsVisibleProperty, new Binding(showSubtime3Binding, stringFormat: "{0:HH:mm:ss}"));
                }

                grid.AddWithSpan(subtime3Label, startRowNo + 4, startColumnNo);
                grid.AddWithSpan(subtime3DisplayText, startRowNo + 4, startColumnNo + 1);

                subTimeTextViews.Add(subtime3Label);
                subTimeDisplayTextViews.Add(subtime3DisplayText);
            }

            if (OperatingSystem.IsWindows())
            {
                return;
            }

            // PRAYER TIME MAIN TITLES
            timeTextViews.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.Instance.GetScreenSizeValue<double>(
                        defaultSize: DebugUtil.GetSizeValues(99)[1],
                        extraLarge: DebugUtil.GetSizeValues(99)[1],
                        large: DebugUtil.GetSizeValues(24)[1],
                        medium: DebugUtil.GetSizeValues(22)[1],
                        small: DebugUtil.GetSizeValues(99)[1],
                        extraSmall: DebugUtil.GetSizeValues(99)[1]);
            });

            // PRAYER TIME MAIN DURATIONS
            timeDisplayTextViews.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.Instance.GetScreenSizeValue<double>(
                        defaultSize: DebugUtil.GetSizeValues(99)[2],
                        extraLarge: DebugUtil.GetSizeValues(99)[2],
                        large: DebugUtil.GetSizeValues(18)[2],
                        medium: DebugUtil.GetSizeValues(14)[2],
                        small: DebugUtil.GetSizeValues(99)[2],
                        extraSmall: DebugUtil.GetSizeValues(99)[2]);
            });

            subTimeTextViews.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.Instance.GetScreenSizeValue<double>(
                        defaultSize: DebugUtil.GetSizeValues(99)[3],
                        extraLarge: DebugUtil.GetSizeValues(99)[3],
                        large: DebugUtil.GetSizeValues(14)[3],
                        medium: DebugUtil.GetSizeValues(12)[3],
                        small: DebugUtil.GetSizeValues(99)[3],
                        extraSmall: DebugUtil.GetSizeValues(99)[3]);
            });

            subTimeDisplayTextViews.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.Instance.GetScreenSizeValue<double>(
                        defaultSize: DebugUtil.GetSizeValues(99)[4],
                        extraLarge: DebugUtil.GetSizeValues(99)[4],
                        large: DebugUtil.GetSizeValues(14)[4],
                        medium: DebugUtil.GetSizeValues(11)[4],
                        small: DebugUtil.GetSizeValues(99)[4],
                        extraSmall: DebugUtil.GetSizeValues(99)[4]);
            });
        }
    }
}
