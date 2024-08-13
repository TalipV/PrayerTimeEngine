using CommunityToolkit.Maui.Storage;
using OnScreenSizeMarkup.Maui.Helpers;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Presentation.GraphicsView;
using PrayerTimeEngine.Presentation.ViewModel;
using UraniumUI.Material.Controls;

namespace PrayerTimeEngine
{
    public partial class MainPage : ContentPage
    {
        private readonly IDispatcher _dispatcher;
        private readonly MainPageViewModel _viewModel;

        public MainPage(IDispatcher dispatcher, MainPageViewModel viewModel)
        {
            _dispatcher = dispatcher;
            BindingContext = this._viewModel = viewModel;
            this.Content = createUI();

            this.BackgroundColor = Color.FromRgb(243, 234, 227);

            viewModel.OnAfterLoadingPrayerTimes_EventTrigger += ViewModel_OnAfterLoadingPrayerTimes_EventTrigger;

            this.lastUpdatedTextInfo.GestureRecognizers
                .Add(
                    new TapGestureRecognizer
                    {
                        Command = new Command(async () => await this.openOptionsMenu().ConfigureAwait(false)),
                        NumberOfTapsRequired = 2,
                    });
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

            this.lastUpdatedTextInfo = new Label
            {
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Center
            };
            lastUpdatedTextInfo.SetBinding(Label.TextProperty, new Binding("PrayerTimeBundle.DataCalculationTimestamp", stringFormat: "{0:dd.MM, HH:mm:ss}"));
            lastUpdatedTextInfo.SetBinding(Label.IsVisibleProperty, nameof(MainPageViewModel.IsNotLoadingPrayerTimesOrSelectedPlace));
            titleGrid.AddWithSpan(lastUpdatedTextInfo, row: 0, column: 0);

            var currentProfileLocationName = new Label
            {
                Padding = new Thickness(0, 0, 20, 0),
                HorizontalTextAlignment = TextAlignment.End,
                VerticalTextAlignment = TextAlignment.Center
            };
            currentProfileLocationName.SetBinding(Label.TextProperty, "CurrentProfile.LocationName");
            currentProfileLocationName.SetBinding(Label.IsVisibleProperty, nameof(MainPageViewModel.IsNotLoadingPrayerTimesOrSelectedPlace));
            titleGrid.AddWithSpan(currentProfileLocationName, row: 0, column: 1);

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
            searchBox.SetBinding(AutoCompleteTextField.IsEnabledProperty, nameof(MainPageViewModel.IsNotLoadingSelectedPlace));
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
                    subtime1Label.SetBinding(Label.IsVisibleProperty, new Binding(showSubtime1Binding, stringFormat: "{0:HH:mm:ss}"));
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
                    subtime1DisplayText.SetBinding(Label.IsVisibleProperty, new Binding(showSubtime1Binding, stringFormat: "{0:HH:mm:ss}"));
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
                    subtime2Label.SetBinding(Label.IsVisibleProperty, new Binding(showSubtime2Binding, stringFormat: "{0:HH:mm:ss}"));
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
                    subtime2DisplayText.SetBinding(Label.IsVisibleProperty, new Binding(showSubtime2Binding, stringFormat: "{0:HH:mm:ss}"));
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
                    subtime3Label.SetBinding(Label.IsVisibleProperty, new Binding(showSubtime3Binding, stringFormat: "{0:HH:mm:ss}"));
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
                    subtime3DisplayText.SetBinding(Label.IsVisibleProperty, new Binding(showSubtime3Binding, stringFormat: "{0:HH:mm:ss}"));
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
                        defaultSize: 99,
                        extraLarge: 99,
                        large: 24,
                        medium: 22,
                        small: 99,
                        extraSmall: 99);
            });

            // PRAYER TIME MAIN DURATIONS
            timeDisplayTextViews.ForEach(label =>
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

            subTimeTextViews.ForEach(label =>
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

            subTimeDisplayTextViews.ForEach(label =>
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
                            sourceFileName: AppConfig.DATABASE_PATH,
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
    }
}
