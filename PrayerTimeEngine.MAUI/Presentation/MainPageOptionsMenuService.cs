using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.ConfigurationManagement;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Pages.Main;
using PrayerTimeEngine.Presentation.Services;
using System.Text.Json;

namespace PrayerTimeEngine.Presentation;

// I just wanted to put this logic somewhere else
internal class MainPageOptionsMenuService(
        MainPage page,
        MainPageViewModel viewModel,
        ToastMessageService toastMessageService,
        IConfigurationImportExportService configurationImportExportService,
        IPreferenceService preferenceService,
        ISystemInfoService systemInfoService,
        ILogger<MainPageOptionsMenuService> logger
    )
{
    private const string _optionsText = "Optionen";

    private const string _generalOptionText = "... Allgemeines";
    private const string _showTimeConfigsOverviewText = "Überblick: Zeiten-Konfiguration";
    private const string _showLocationConfigsOverviewText = "Überblick: Ortsdaten";
    private const string _showLogsText = "Logs anzeigen";
    private const string _setCustomTextSizes = "Benutzerdefinierte Textgröße";
    private const string _exportConfiguration = "Konfiguration exportieren";
    private const string _importConfiguration = "Konfiguration importieren";

    private const string _technicalOptionText = "... Technisches";
    private const string _showDbTablesText = "DB-Tabellen anzeigen";
    private const string _saveDbFileText = "DB-Datei speichern";
    private const string _deviceInfoText = "Geräte-Informationen";

    private const string _systemOptionText = "... System";
    private const string _resetAppText = "App-Daten zurücksetzen";
    private const string _closeAppText = "App schließen";

    private const string _goldPriceText = "Tool: Goldpreise";

    private const string _backText = "Zurück";
    private const string _cancelText = "Abbrechen";

    public async Task OpenGeneralOptionsMenu()
    {
        if (viewModel.CurrentProfileWithModel == null)
        {
            await page.DisplayAlertAsync("Abbruch", "CurrentProfileWithModel ist NULL??", "OK");
            return;
        }

        CancellationToken cancellationToken = CancellationToken.None;

        bool doRepeat;
        try
        {
            do
            {
                doRepeat = false;

                switch (await page.DisplayActionSheetAsync(
                    title: _optionsText,
                    cancel: _cancelText,
                    destruction: null,
                    _generalOptionText,
                    _technicalOptionText,
                    _systemOptionText,
                    _goldPriceText))
                {
                    case _generalOptionText:

                        switch (await page.DisplayActionSheetAsync(
                            title: _generalOptionText,
                            cancel: _backText,
                            destruction: null,
                            _showTimeConfigsOverviewText,
                            _showLocationConfigsOverviewText,
                            _showLogsText,
                            _setCustomTextSizes,
                            _exportConfiguration, 
                            _importConfiguration))
                        {
                            case _showTimeConfigsOverviewText:
                                await page.DisplayAlertAsync("Info", viewModel.GetPrayerTimeConfigDisplayText(), "Ok");
                                break;
                            case _showLocationConfigsOverviewText:
                                await page.DisplayAlertAsync("Info", viewModel.GetLocationDataDisplayText(), "Ok");
                                break;
                            case _showLogsText:
                                viewModel.GoToLogsPageCommand.Execute(null);
                                break;
                            case _setCustomTextSizes:
                                showCustomTextSizesInputPopup();
                                break;
                            case _exportConfiguration:
                                await exportConfiguration(cancellationToken);
                                break;
                            case _importConfiguration:
                                await importConfiguration(cancellationToken);
                                break;
                            case _backText:
                                doRepeat = true;
                                break;
                        }

                        break;

                    case _technicalOptionText:

                        switch (await page.DisplayActionSheetAsync(
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
                                await page.DisplayAlertAsync(
                                    "Geräteinformationen",
                                    $"""
                                    Modell: {DeviceInfo.Manufacturer.ToUpper()}, {DeviceInfo.Model}
                                    Art: {DeviceInfo.Idiom}, {DeviceInfo.DeviceType}
                                    OS: {DeviceInfo.Platform}, {DeviceInfo.VersionString}
                                    Auflösung: {DeviceDisplay.MainDisplayInfo.Height}x{DeviceDisplay.MainDisplayInfo.Width} (Dichte: {DeviceDisplay.MainDisplayInfo.Density})
                                    Kategorie der Größe: {DebugUtil.GetScreenSizeCategoryName()}
                                    Zeitzone: {systemInfoService.GetSystemTimeZone().Id}
                                """
                                    , "Ok");
                                break;
                            case _backText:
                                doRepeat = true;
                                break;
                        }

                        break;
                    case _systemOptionText:

                        switch (await page.DisplayActionSheetAsync(
                            title: _systemOptionText,
                            cancel: _backText,
                            destruction: null,
                            _resetAppText,
                            _closeAppText))
                        {
                            case _resetAppText:
                                if (!await page.DisplayAlertAsync("Bestätigung", "Daten wirklich zurücksetzen?", "Ja", _cancelText))
                                    break;

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
                    case _goldPriceText:
                        decimal goldEurPricePerGram = await getGoldGramEurAsync("fcdc9197a43a72f497fe20e4131542c0");
                        await page.DisplayAlertAsync("Info", $"""
                            Goldpreis pro Gramm: {goldEurPricePerGram:N2}€
                            --> Nisab beträgt {(GOLD_NISAB_GRAMM * goldEurPricePerGram):N2}€ ({GOLD_NISAB_GRAMM:N2} g)
                            """, "Ok");
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

    private const decimal GOLD_NISAB_GRAMM = 84.7M;

    private static async Task<decimal> getGoldGramEurAsync(string apiKey)
    {
        using var http = new HttpClient();
        string url = $"https://api.metalpriceapi.com/v1/latest?api_key={apiKey}&base=EUR&currencies=XAU";
        string json = await http.GetStringAsync(url);

        JsonDocument data = System.Text.Json.JsonDocument.Parse(json);
        decimal eurPerOunce = data.RootElement
            .GetProperty("rates")
            .GetProperty("EURXAU")
            .GetDecimal();

        decimal eurPerGramm = eurPerOunce / 31.1034768m;

        return eurPerGramm;
    }
    

    public async Task OpenProfileOptionsMenu()
    {
        List<string> options = [
                "Neues Profil erstellen",
                "Neues Moschee-Profil erstellen",
                "Profilnamen bearbeiten",
                "Profil löschen"
            ];

        if (viewModel.CurrentProfileWithModel == null)
        {
            await page.DisplayAlertAsync("Abbruch", "CurrentProfileWithModel ist NULL??", "OK");
            return;
        }

        if (viewModel.CurrentProfile is MosqueProfile mosqueProfile)
        {
            options.Add("Internetseite der Moschee-Zeiten öffnen");
        }

        switch (await page.DisplayActionSheetAsync(
                            title: "Profilverwaltung",
                            cancel: "Abbrechen",
                            destruction: null,
                            buttons: [.. options]))
        {
            case "Neues Profil erstellen":
                await viewModel.CreateNewProfile();
                break;

            case "Neues Moschee-Profil erstellen":

                var items = Enum.GetValues<EMosquePrayerTimeProviderType>().ToList();
                items.Remove(EMosquePrayerTimeProviderType.None);

                string selectedItemText = await page.DisplayActionSheetAsync(
                    title: "Moschee-App auswählen",
                    cancel: "Abbrechen",
                    destruction: null,
                    items.Select(x => x.ToString()).ToArray()
                   );

                EMosquePrayerTimeProviderType selectedItem = items.FirstOrDefault(x => x.ToString() == selectedItemText);

                if (selectedItem != EMosquePrayerTimeProviderType.None)
                {
                    string externalID = await page.DisplayPromptAsync(
                        title: "Kennung",
                        message: "Kennung der jeweiligen Moschee eingeben",
                        initialValue: "",
                        keyboard: Keyboard.Text) ?? "";

                    await viewModel.CreateNewMosqueProfile(selectedItem, externalID);
                }

                break;

            case "Profilnamen bearbeiten":
                string currentProfileName = viewModel.CurrentProfile?.Name ?? "";
                string newProfileName =
                    await page.DisplayPromptAsync("Profilname:",
                    message: "",
                    initialValue: currentProfileName,
                    keyboard: Keyboard.Text) ?? "";

                if (!string.IsNullOrWhiteSpace(newProfileName))
                {
                    await viewModel.ChangeProfileName(newProfileName);
                }

                break;

            case "Profil löschen":
                await viewModel.DeleteCurrentProfile();
                break;

            case "Internetseite der Moschee-Zeiten öffnen":
                await viewModel.OpenMosqueInternetPage();
                break;
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

            await page.DisplayAlertAsync("Erfolg", $"Gespeichert! App manuell wieder starten!", "OK");
            Application.Current.Quit();
        }
        else
        {
            await page.DisplayAlertAsync("Eingabe ungültig", "Eingabe war ungültig", "OK");
        }
    }

    private async Task exportConfiguration(CancellationToken cancellationToken)
    {
        Profile[] profiles = viewModel.ProfilesWithModel.Select(x => x.Profile).ToArray();

        string serializedConfiguration = configurationImportExportService.SerializeConfiguration(new Configuration
        {
            Profiles = profiles
        });

        FileSaverResult result = null;

        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(serializedConfiguration)))
        {
            result = await FileSaver.Default.SaveAsync(
                $"PrayerTimeEngine_Config_{systemInfoService.GetCurrentZonedDateTime().ToString("dd_MM_yyyy_HH:mm", null)}.txt",
                stream,
                cancellationToken
            );
        }

        if (result.IsSuccessful)
        {
            await page.DisplayAlertAsync("Erfolg", "Der Export der Konfiguration war erfolgreich", "OK");
        }
        else
        {
            logger.LogError(
                result.Exception,
                "Error while writing export configuration to destination '{DestinationFilePath}'",
                result.FilePath);

            await page.DisplayAlertAsync("Fehler", $"Fehler beim Exportieren: {result.Exception?.Message ?? "-"}", "OK");
        }
    }

    private static readonly FilePickerFileType _configImportFilePickerFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.WinUI, new[] { ".txt" } },
        { DevicePlatform.MacCatalyst, new[] { "public.plain-text" } },
        { DevicePlatform.iOS, new[] { "public.plain-text" } },
        { DevicePlatform.Android, new[] { "text/plain" } }
    });

    private static readonly PickOptions _configImportPickOptions = new PickOptions
    {
        PickerTitle = "Bitte wählen Sie die Konfigurationsdatei aus",
        FileTypes = _configImportFilePickerFileType
    };

    private async Task importConfiguration(CancellationToken cancellationToken)
    {
        FileResult pickedFile = await FilePicker.Default.PickAsync(_configImportPickOptions);

        if (pickedFile == null)
        {
            await page.DisplayAlertAsync("Abbruch", "Es wurde keine Datei ausgewählt", "OK");
            return;
        }

        try
        {
            string fileContent;
            using (Stream stream = await pickedFile.OpenReadAsync())
            using (var reader = new StreamReader(stream))
            {
                fileContent = await reader.ReadToEndAsync(cancellationToken);
            }

            await configurationImportExportService.Import(fileContent, cancellationToken);

            await page.DisplayAlertAsync("Erfolg", "Der Import der Konfiguration war erfolgreich", "OK");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Error while importing configuration '{FilePath}'",
                pickedFile.FullPath);

            await page.DisplayAlertAsync("Fehler", $"Fehler beim Exportieren: {exception?.Message ?? "-"}", "OK");
        }
    }

}
