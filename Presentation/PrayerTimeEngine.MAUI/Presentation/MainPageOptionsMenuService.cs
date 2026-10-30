using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.ConfigurationManagement;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Pages.Main;
using PrayerTimeEngine.Presentation.Pages.QiblahFinder;
using PrayerTimeEngine.Presentation.Popups;
using PrayerTimeEngine.Presentation.Services;
using PrayerTimeEngine.Presentation.Services.Navigation;
using PrayerTimeEngine.Presentation.Localization;

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
    private enum EOptionsMenuAction
    {
        ChangeProfileName,
        CreateProfile,
        CreateMosqueProfile,
        OpenMosqueWebPage,
        DeleteProfile,
        TimeConfigurationOverview,
        LocationOverview,
        Qiblah,
        GoldPrices,
        ExportConfiguration,
        ImportConfiguration,
        ShowLogs,
        ShowDatabaseTables,
        SaveDatabaseFile,
        DeviceInformation,
        ResetApp,
        CloseApp,
    }

    private List<OptionsMenuSection> CreateMenuSections()
    {
        List<OptionsMenuItem> profileItems =
        [
            new(nameof(EOptionsMenuAction.ChangeProfileName), AppResources.Menu_ChangeProfileName),
            new(nameof(EOptionsMenuAction.CreateProfile), AppResources.Menu_CreateProfile),
            new(nameof(EOptionsMenuAction.CreateMosqueProfile), AppResources.Menu_CreateMosqueProfile),
        ];

        if (viewModel.CurrentProfile is MosqueProfile)
        {
            profileItems.Add(new(nameof(EOptionsMenuAction.OpenMosqueWebPage), AppResources.Menu_OpenMosqueWebPage));
        }

        profileItems.Add(new(nameof(EOptionsMenuAction.DeleteProfile), AppResources.Menu_DeleteProfile, IsDestructive: true));

        List<OptionsMenuSection> sections = [new(AppResources.Menu_SectionProfile, profileItems)];

        // both overviews are only implemented for profiles with calculated times
        if (viewModel.CurrentProfile is DynamicProfile)
        {
            sections.Add(new(AppResources.Menu_SectionOverview,
            [
                new(nameof(EOptionsMenuAction.TimeConfigurationOverview), AppResources.Menu_TimeConfigurationOverview),
                new(nameof(EOptionsMenuAction.LocationOverview), AppResources.Menu_LocationOverview),
            ]));
        }

        sections.Add(new(AppResources.Menu_SectionTools,
        [
            new(nameof(EOptionsMenuAction.Qiblah), AppResources.Menu_Qiblah),
            new(nameof(EOptionsMenuAction.GoldPrices), AppResources.Menu_GoldPrices),
        ]));
        sections.Add(new(AppResources.Menu_SectionConfiguration,
        [
            new(nameof(EOptionsMenuAction.ExportConfiguration), AppResources.Menu_ExportConfiguration),
            new(nameof(EOptionsMenuAction.ImportConfiguration), AppResources.Menu_ImportConfiguration),
        ]));
        sections.Add(new(AppResources.Menu_SectionTechnical,
        [
            new(nameof(EOptionsMenuAction.ShowLogs), AppResources.Menu_ShowLogs),
            new(nameof(EOptionsMenuAction.ShowDatabaseTables), AppResources.Menu_ShowDatabaseTables),
            new(nameof(EOptionsMenuAction.SaveDatabaseFile), AppResources.Menu_SaveDatabaseFile),
            new(nameof(EOptionsMenuAction.DeviceInformation), AppResources.Menu_DeviceInformation),
        ]));
        sections.Add(new(AppResources.Menu_SectionSystem,
        [
            new(nameof(EOptionsMenuAction.ResetApp), AppResources.Menu_ResetApp, IsDestructive: true),
            new(nameof(EOptionsMenuAction.CloseApp), AppResources.Menu_CloseApp),
        ]));

        return sections;
    }

    public async Task OpenOptionsMenu()
    {
        if (viewModel.CurrentProfileWithModel == null)
        {
            await page.DisplayAlertAsync(AppResources.Common_Aborted, "CurrentProfileWithModel ist NULL??", AppResources.Common_Ok);
            return;
        }

        CancellationToken cancellationToken = CancellationToken.None;

        try
        {
            var menuPopup = new OptionsMenuPopup(AppResources.Menu_Title, viewModel.CurrentProfile?.Name, CreateMenuSections());
            IPopupResult<string?> menuResult = await page.ShowPopupAsync<string?>(menuPopup, OptionsMenuPopup.CreatePopupOptions(), cancellationToken);

            if (!Enum.TryParse(menuResult.Result, out EOptionsMenuAction action))
            {
                return;
            }

            switch (action)
            {
                case EOptionsMenuAction.ChangeProfileName:
                    string currentProfileName = viewModel.CurrentProfile?.Name ?? "";
                    string newProfileName =
                        await page.DisplayPromptAsync(AppResources.Menu_ProfileNamePrompt,
                        message: "",
                        initialValue: currentProfileName,
                        keyboard: Keyboard.Text) ?? "";

                    if (!string.IsNullOrWhiteSpace(newProfileName))
                    {
                        await viewModel.ChangeProfileName(newProfileName);
                    }

                    break;

                case EOptionsMenuAction.CreateProfile:
                    await viewModel.CreateNewProfile();
                    break;

                case EOptionsMenuAction.CreateMosqueProfile:

                    List<EMosquePrayerTimeProviderType> items = [.. Enum.GetValues<EMosquePrayerTimeProviderType>()];
                    items.Remove(EMosquePrayerTimeProviderType.None);

                    string selectedItemText = await page.DisplayActionSheetAsync(
                        title: AppResources.Menu_SelectMosqueApp,
                        cancel: AppResources.Common_Cancel,
                        destruction: null,
                        [.. items.Select(x => x.ToString())]
                       );

                    EMosquePrayerTimeProviderType selectedItem = items.FirstOrDefault(x => x.ToString() == selectedItemText);

                    if (selectedItem != EMosquePrayerTimeProviderType.None)
                    {
                        string externalID = await page.DisplayPromptAsync(
                            title: AppResources.Menu_MosqueIdentifierTitle,
                            message: AppResources.Menu_MosqueIdentifierMessage,
                            initialValue: "",
                            keyboard: Keyboard.Text) ?? "";

                        await viewModel.CreateNewMosqueProfile(selectedItem, externalID);
                    }

                    break;

                case EOptionsMenuAction.OpenMosqueWebPage:
                    await viewModel.OpenMosqueInternetPage();
                    break;

                case EOptionsMenuAction.DeleteProfile:
                    // the entry now sits directly in the menu instead of one level down, so it gets a confirmation
                    if (await page.DisplayAlertAsync(
                        AppResources.Common_Confirmation,
                        string.Format(AppResources.Menu_DeleteProfileConfirmation, viewModel.CurrentProfile?.Name),
                        AppResources.Menu_Delete,
                        AppResources.Common_Cancel))
                    {
                        await viewModel.DeleteCurrentProfile();
                    }

                    break;

                case EOptionsMenuAction.TimeConfigurationOverview:
                    await page.DisplayAlertAsync(AppResources.Menu_TimeConfigurationOverview, viewModel.GetPrayerTimeConfigDisplayText(), AppResources.Common_Ok);
                    break;

                case EOptionsMenuAction.LocationOverview:
                    await ShowLocationOverview();
                    break;

                case EOptionsMenuAction.Qiblah:
                    await navigationService.NavigateTo<QiblahMapPage>();
                    break;

                case EOptionsMenuAction.GoldPrices:
                    decimal goldEurPricePerGram = await GetGoldGramEurAsync(AppApiKeys.MetalPrice);
                    decimal silverEurPricePerGram = await GetSilverGramEurAsync(AppApiKeys.MetalPrice);
                    await page.DisplayAlertAsync(
                        AppResources.Menu_GoldPrices,
                        string.Format(
                            AppResources.Menu_GoldPricesText,
                            goldEurPricePerGram,
                            GOLD_NISAB_GRAMM * goldEurPricePerGram,
                            GOLD_NISAB_GRAMM,
                            silverEurPricePerGram,
                            SILVER_NISAB_GRAMM * silverEurPricePerGram,
                            SILVER_NISAB_GRAMM),
                        AppResources.Common_Ok);
                    break;

                case EOptionsMenuAction.ExportConfiguration:
                    await ExportConfiguration(cancellationToken);
                    break;

                case EOptionsMenuAction.ImportConfiguration:
                    await ImportConfiguration(cancellationToken);
                    break;

                case EOptionsMenuAction.ShowLogs:
                    viewModel.GoToLogsPageCommand.Execute(null);
                    break;

                case EOptionsMenuAction.ShowDatabaseTables:
                    await viewModel.ShowDatabaseTable();
                    break;

                case EOptionsMenuAction.SaveDatabaseFile:
                    FolderPickerResult folderPickerResult = await FolderPicker.PickAsync(CancellationToken.None);

                    if (folderPickerResult.Folder is not null)
                    {
                        File.Copy(
                            sourceFileName: AppConfig.DATABASE_PATH,
                            destFileName: Path.Combine(folderPickerResult.Folder.Path, $"dbFile_{DateTime.Now:ddMMyyyy_HH_mm}.db"),
                            overwrite: true);
                    }

                    break;

                case EOptionsMenuAction.DeviceInformation:
                    await page.DisplayAlertAsync(
                        AppResources.Menu_DeviceInformation,
                        string.Format(
                            AppResources.Menu_DeviceInformationText,
                            DeviceInfo.Manufacturer.ToUpper(),
                            DeviceInfo.Model,
                            DeviceInfo.Idiom,
                            DeviceInfo.DeviceType,
                            DeviceInfo.Platform,
                            DeviceInfo.VersionString,
                            DeviceDisplay.MainDisplayInfo.Height,
                            DeviceDisplay.MainDisplayInfo.Width,
                            DeviceDisplay.MainDisplayInfo.Density,
                            systemInfoService.GetSystemTimeZone().Id),
                        AppResources.Common_Ok);
                    break;

                case EOptionsMenuAction.ResetApp:
                    if (!await page.DisplayAlertAsync(AppResources.Common_Confirmation, AppResources.Menu_ResetAppConfirmation, AppResources.Common_Yes, AppResources.Common_Cancel))
                    {
                        break;
                    }

                    preferenceService.SetDoReset();

                    Application.Current.Quit();
                    break;

                case EOptionsMenuAction.CloseApp:
                    Application.Current.Quit();
                    break;
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error in options menu");
            toastMessageService.ShowError(exception.Message);
        }
    }

    /// <summary>
    /// The location overview, with a button that opens the coordinates in a map app so it is
    /// easy to check where they actually point to.
    /// </summary>
    private async Task ShowLocationOverview()
    {
        string locationText = viewModel.GetLocationDataDisplayText();
        (decimal Latitude, decimal Longitude)? coordinates = viewModel.GetLocationCoordinates();

        if (coordinates is null)
        {
            await page.DisplayAlertAsync(AppResources.Menu_LocationOverview, locationText, AppResources.Common_Ok);
            return;
        }

        if (await page.DisplayAlertAsync(AppResources.Menu_LocationOverview, locationText, AppResources.Menu_OpenInMap, AppResources.Common_Ok))
        {
            await viewModel.OpenCoordinatesInMapApp(coordinates.Value.Latitude, coordinates.Value.Longitude);
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
            await page.DisplayAlertAsync(AppResources.Common_Success, AppResources.Menu_ExportSuccessful, AppResources.Common_Ok);
        }
        else
        {
            logger.LogError(result.Exception,
                "Error while writing export configuration to destination '{DestinationFilePath}'",
                result.FilePath);

            await page.DisplayAlertAsync(AppResources.Common_Error, string.Format(AppResources.Menu_ExportFailed, result.Exception?.Message ?? "-"), AppResources.Common_Ok);
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
        PickerTitle = AppResources.Menu_SelectConfigurationFile,
        FileTypes = s_configImportFilePickerFileType
    };

    private async Task ImportConfiguration(CancellationToken cancellationToken)
    {
        FileResult? pickedFile = await FilePicker.Default.PickAsync(s_configImportPickOptions);

        if (pickedFile == null)
        {
            await page.DisplayAlertAsync(AppResources.Common_Aborted, AppResources.Menu_NoFileSelected, AppResources.Common_Ok);
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

            await page.DisplayAlertAsync(AppResources.Common_Success, AppResources.Menu_ImportSuccessful, AppResources.Common_Ok);
            await viewModel.ReloadAfterConfigurationImport();
        }
        catch (Exception exception)
        {
            logger.LogError(exception,
                "Error while importing configuration '{FilePath}'",
                pickedFile.FullPath);

            await page.DisplayAlertAsync(AppResources.Common_Error, string.Format(AppResources.Menu_ImportFailed, exception?.Message ?? "-"), AppResources.Common_Ok);
        }
    }

}
