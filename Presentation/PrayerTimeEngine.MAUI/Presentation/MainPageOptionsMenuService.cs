using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.ConfigurationManagement;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Pages.Main;
using PrayerTimeEngine.Presentation.Pages.QiblahFinder;
using PrayerTimeEngine.Presentation.Services;
using PrayerTimeEngine.Presentation.Services.Navigation;

namespace PrayerTimeEngine.Presentation;

// I just wanted to put this logic somewhere else
internal class MainPageOptionsMenuService(
        MainPage page,
        MainPageViewModel viewModel,
        ToastMessageService toastMessageService,
        INavigationService navigationService,
        IConfigurationImportExportService configurationImportExportService,
        IPreferenceService preferenceService,
        ISystemInfoService systemInfoService,
        ILogger<MainPageOptionsMenuService> logger
    )
{
    private const string OptionsText = "Optionen";

    private const string GeneralOptionText = "... Allgemeines";
    private const string ShowTimeConfigsOverviewText = "Überblick: Zeiten-Konfiguration";
    private const string ShowLocationConfigsOverviewText = "Überblick: Ortsdaten";
    private const string ShowLogsText = "Logs anzeigen";
    private const string ExportConfigurationOptionsText = "Konfiguration exportieren";
    private const string ImportConfigurationOptionsText = "Konfiguration importieren";

    private const string ProfileOptionsText = "... Profilverwaltung";
    private const string CreateNewDynamicProfileOptionsText = "Neues Profil erstellen";
    private const string CreateNewMosqueProfileOptionsText = "Neues Moschee-Profil erstellen";
    private const string ChangeProfileNameOptionsText = "Profilnamen bearbeiten";
    private const string DeleteProfileOptionsText = "Profil löschen";
    private const string OpenMosqueProfileWebPageOptionsText = "Internetseite der Moschee-Zeiten öffnen";

    private const string TechnicalOptionText = "... Technisches";
    private const string ShowDbTablesText = "DB-Tabellen anzeigen";
    private const string SaveDbFileText = "DB-Datei speichern";
    private const string DeviceInfoText = "Geräte-Informationen";

    private const string SystemOptionText = "... System";
    private const string ResetAppText = "App-Daten zurücksetzen";
    private const string CloseAppText = "App schließen";

    private const string GoldPriceText = "Tool: Goldpreise";
    private const string QiblahToolText = "Tool: Qiblah";

    private const string BackText = "Zurück";
    private const string CancelText = "Abbrechen";

    public async Task OpenOptionsMenu()
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
                    title: OptionsText,
                    cancel: CancelText,
                    destruction: null,
                    GeneralOptionText,
                    ProfileOptionsText,
                    TechnicalOptionText,
                    SystemOptionText,
                    GoldPriceText,
                    QiblahToolText))
                {
                    case GeneralOptionText:

                        switch (await page.DisplayActionSheetAsync(
                            title: GeneralOptionText,
                            cancel: BackText,
                            destruction: null,
                            ShowTimeConfigsOverviewText,
                            ShowLocationConfigsOverviewText,
                            ShowLogsText,
                            ExportConfigurationOptionsText,
                            ImportConfigurationOptionsText))
                        {
                            case ShowTimeConfigsOverviewText:
                                await page.DisplayAlertAsync("Info", viewModel.GetPrayerTimeConfigDisplayText(), "Ok");
                                break;
                            case ShowLocationConfigsOverviewText:
                                await page.DisplayAlertAsync("Info", viewModel.GetLocationDataDisplayText(), "Ok");
                                break;
                            case ShowLogsText:
                                viewModel.GoToLogsPageCommand.Execute(null);
                                break;
                            case ExportConfigurationOptionsText:
                                await ExportConfiguration(cancellationToken);
                                break;
                            case ImportConfigurationOptionsText:
                                await ImportConfiguration(cancellationToken);
                                break;
                            case BackText:
                                doRepeat = true;
                                break;
                        }

                        break;

                    case ProfileOptionsText:

                        List<string> options = [
                            CreateNewDynamicProfileOptionsText,
                            CreateNewMosqueProfileOptionsText,
                            ChangeProfileNameOptionsText,
                            DeleteProfileOptionsText,
                        ];

                        if (viewModel.CurrentProfile is MosqueProfile mosqueProfile)
                        {
                            options.Add(OpenMosqueProfileWebPageOptionsText);
                        }

                        switch (await page.DisplayActionSheetAsync(
                            title: ProfileOptionsText,
                            cancel: BackText,
                            destruction: null,
                            buttons: [.. options]))
                        {
                            case CreateNewDynamicProfileOptionsText:
                                await viewModel.CreateNewProfile();
                                break;

                            case CreateNewMosqueProfileOptionsText:

                                List<EMosquePrayerTimeProviderType> items = [.. Enum.GetValues<EMosquePrayerTimeProviderType>()];
                                items.Remove(EMosquePrayerTimeProviderType.None);

                                string selectedItemText = await page.DisplayActionSheetAsync(
                                    title: "Moschee-App auswählen",
                                    cancel: "Abbrechen",
                                    destruction: null,
                                    [.. items.Select(x => x.ToString())]
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

                            case ChangeProfileNameOptionsText:
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

                            case DeleteProfileOptionsText:
                                await viewModel.DeleteCurrentProfile();
                                break;

                            case OpenMosqueProfileWebPageOptionsText:
                                await viewModel.OpenMosqueInternetPage();
                                break;
                            case BackText:
                                doRepeat = true;
                                break;
                        }

                        break;

                    case TechnicalOptionText:

                        switch (await page.DisplayActionSheetAsync(
                            title: TechnicalOptionText,
                            cancel: BackText,
                            destruction: null,
                            ShowDbTablesText,
                            SaveDbFileText,
                            DeviceInfoText))
                        {
                            case ShowDbTablesText:
                                await viewModel.ShowDatabaseTable();
                                break;
                            case SaveDbFileText:
                                FolderPickerResult folderPickerResult = await FolderPicker.PickAsync(CancellationToken.None);

                                if (folderPickerResult.Folder is not null)
                                {
                                    File.Copy(
                                        sourceFileName: AppConfig.DATABASE_PATH,
                                        destFileName: Path.Combine(folderPickerResult.Folder.Path, $"dbFile_{DateTime.Now:ddMMyyyy_HH_mm}.db"),
                                        overwrite: true);
                                }

                                break;

                            case DeviceInfoText:
                                await page.DisplayAlertAsync(
                                    "Geräteinformationen",
                                    $"""
                                    Modell: {DeviceInfo.Manufacturer.ToUpper()}, {DeviceInfo.Model}
                                    Art: {DeviceInfo.Idiom}, {DeviceInfo.DeviceType}
                                    OS: {DeviceInfo.Platform}, {DeviceInfo.VersionString}
                                    Auflösung: {DeviceDisplay.MainDisplayInfo.Height}x{DeviceDisplay.MainDisplayInfo.Width} (Dichte: {DeviceDisplay.MainDisplayInfo.Density})
                                    Zeitzone: {systemInfoService.GetSystemTimeZone().Id}
                                """
                                    , "Ok");
                                break;
                            case BackText:
                                doRepeat = true;
                                break;
                        }

                        break;
                    case SystemOptionText:

                        switch (await page.DisplayActionSheetAsync(
                            title: SystemOptionText,
                            cancel: BackText,
                            destruction: null,
                            ResetAppText,
                            CloseAppText))
                        {
                            case ResetAppText:
                                if (!await page.DisplayAlertAsync("Bestätigung", "Daten wirklich zurücksetzen?", "Ja", CancelText))
                                {
                                    break;
                                }

                                preferenceService.SetDoReset();

                                Application.Current.Quit();
                                break;
                            case CloseAppText:
                                Application.Current.Quit();
                                break;
                            case BackText:
                                doRepeat = true;
                                break;
                        }

                        break;
                    case GoldPriceText:
                        decimal goldEurPricePerGram = await GetGoldGramEurAsync(AppApiKeys.MetalPrice);
                        decimal silverEurPricePerGram = await GetSilverGramEurAsync(AppApiKeys.MetalPrice);
                        await page.DisplayAlertAsync("Info", $"""
                            Goldpreis pro Gramm: {goldEurPricePerGram:N5}€
                            --> Nisab beträgt {GOLD_NISAB_GRAMM * goldEurPricePerGram:N2}€ ({GOLD_NISAB_GRAMM:N2} g)

                            Silberpreis pro Gramm: {silverEurPricePerGram:N5}€
                            --> Nisab beträgt {SILVER_NISAB_GRAMM * silverEurPricePerGram:N2}€ ({SILVER_NISAB_GRAMM:N2} g)
                            """, "Ok");
                        break;
                    case QiblahToolText:
                        await navigationService.NavigateTo<QiblahMapPage>();
                        break;
                    case CancelText:
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
    private const decimal SILVER_NISAB_GRAMM = 592.9M;

    private static async Task<decimal> GetGoldGramEurAsync(string apiKey)
    {
        // XAU --> gold

        using var http = new HttpClient();
        string url = $"https://api.metalpriceapi.com/v1/latest?api_key={apiKey}&base=EUR&currencies=XAU";
        string json = await http.GetStringAsync(url);

        var data = System.Text.Json.JsonDocument.Parse(json);
        decimal eurPerOunce = data.RootElement
            .GetProperty("rates")
            .GetProperty("EURXAU")
            .GetDecimal();

        decimal eurPerGramm = eurPerOunce / 31.1034768m;

        return eurPerGramm;
    }

    private static async Task<decimal> GetSilverGramEurAsync(string apiKey)
    {
        // XAG --> silver

        using var http = new HttpClient();
        string url = $"https://api.metalpriceapi.com/v1/latest?api_key={apiKey}&base=EUR&currencies=XAG";
        string json = await http.GetStringAsync(url);

        var data = System.Text.Json.JsonDocument.Parse(json);
        decimal eurPerOunce = data.RootElement
            .GetProperty("rates")
            .GetProperty("EURXAG")
            .GetDecimal();

        decimal eurPerGramm = eurPerOunce / 31.1034768m;

        return eurPerGramm;
    }

    private async Task ExportConfiguration(CancellationToken cancellationToken)
    {
        Profile[] profiles = [.. viewModel.ProfilesWithModel.Select(x => x.Profile)];

        string serializedConfiguration = configurationImportExportService.SerializeConfiguration(new Configuration
        {
            Profiles = profiles
        });

        FileSaverResult? result = null;

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
            logger.LogError(result.Exception,
                "Error while writing export configuration to destination '{DestinationFilePath}'",
                result.FilePath);

            await page.DisplayAlertAsync("Fehler", $"Fehler beim Exportieren: {result.Exception?.Message ?? "-"}", "OK");
        }
    }

    private static readonly FilePickerFileType s_configImportFilePickerFileType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.WinUI, new[] { ".txt" } },
        { DevicePlatform.MacCatalyst, new[] { "public.plain-text" } },
        { DevicePlatform.iOS, new[] { "public.plain-text" } },
        { DevicePlatform.Android, new[] { "text/plain" } }
    });

    private static readonly PickOptions s_configImportPickOptions = new()
    {
        PickerTitle = "Bitte wählen Sie die Konfigurationsdatei aus",
        FileTypes = s_configImportFilePickerFileType
    };

    private async Task ImportConfiguration(CancellationToken cancellationToken)
    {
        FileResult? pickedFile = await FilePicker.Default.PickAsync(s_configImportPickOptions);

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

            await page.DisplayAlertAsync("Erfolg", "Import erfolgreich!", "OK");
            await viewModel.ReloadAfterConfigurationImport();
        }
        catch (Exception exception)
        {
            logger.LogError(exception,
                "Error while importing configuration '{FilePath}'",
                pickedFile.FullPath);

            await page.DisplayAlertAsync("Fehler", $"Fehler beim Exportieren: {exception?.Message ?? "-"}", "OK");
        }
    }

}
