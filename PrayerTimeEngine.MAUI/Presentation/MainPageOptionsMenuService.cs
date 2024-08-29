﻿using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Presentation.Service;
using PrayerTimeEngine.Presentation.ViewModel;
using PrayerTimeEngine.Presentation.Views;

namespace PrayerTimeEngine.Presentation
{
    // I just wanted to put this logic somewhere else
    internal class MainPageOptionsMenuService(
            ToastMessageService toastMessageService, 
            MainPage page,
            MainPageViewModel viewModel,
            ILogger<MainPageOptionsMenuService> logger,
            IPreferenceService preferenceService
        )
    {
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

        public async Task openOptionsMenu()
        {
            bool doRepeat;

            try
            {
                do
                {
                    doRepeat = false;

                    switch (await page.DisplayActionSheet(
                        title: _optionsText,
                        cancel: _cancelText,
                        destruction: null,
                        _generalOptionText,
                        _technicalOptionText,
                        _systemOptionText))
                    {
                        case _generalOptionText:

                            switch (await page.DisplayActionSheet(
                                title: _generalOptionText,
                                cancel: _backText,
                                destruction: null,
                                _showTimeConfigsOverviewText,
                                _showLocationConfigsOverviewText,
                                _showLogsText,
                                _setCustomTextSizes))
                            {
                                case _showTimeConfigsOverviewText:
                                    await page.DisplayAlert("Info", viewModel.GetPrayerTimeConfigDisplayText(), "Ok");
                                    break;
                                case _showLocationConfigsOverviewText:
                                    await page.DisplayAlert("Info", viewModel.GetLocationDataDisplayText(), "Ok");
                                    break;
                                case _showLogsText:
                                    viewModel.GoToLogsPageCommand.Execute(null);
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

                            switch (await page.DisplayActionSheet(
                                title: _technicalOptionText,
                                cancel: _backText,
                                destruction: null,
                                _showDbTablesText,
                                _saveDbFileText,
                                _deviceInfoText))
                            {
                                case _showDbTablesText:
                                    await viewModel.ShowDatabaseTable();
                                    break;
                                case _saveDbFileText:
                                    FolderPickerResult folderPickerResult = await FolderPicker.PickAsync(CancellationToken.None);

                                    if (folderPickerResult.Folder is not null)
                                    {
                                        File.Copy(
                                            sourceFileName: AppConfig.DATABASE_PATH,
                                            destFileName: Path.Combine(folderPickerResult.Folder.Path, $"dbFile_{DateTime.Now:ddMMyyyy_HH_mm}.db"),
                                            overwrite: true);
                                    }

                                    break;

                                case _deviceInfoText:
                                    await page.DisplayAlert(
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

                            switch (await page.DisplayActionSheet(
                                title: _systemOptionText,
                                cancel: _backText,
                                destruction: null,
                                _resetAppText,
                                _closeAppText))
                            {
                                case _resetAppText:
                                    if (!await page.DisplayAlert("Bestätigung", "Daten wirklich zurücksetzen?", "Ja", _cancelText))
                                        break;

                                    //if (File.Exists(AppConfig.DATABASE_PATH))
                                    //    File.Delete(AppConfig.DATABASE_PATH);

                                    preferenceService.SetDoReset();

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
            catch (Exception exception)
            {
                logger.LogError(exception, "Error in options menu");
                toastMessageService.ShowError(exception.Message);
            }
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
                await page.DisplayPromptAsync(
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

                await page.DisplayAlert("Erfolg", $"Gespeichert! App manuell wieder starten!", "OK");
                Application.Current.Quit();
            }
            else
            {
                await page.DisplayAlert("Eingabe ungültig", "Eingabe war ungültig", "OK");
            }
        }
    }
}