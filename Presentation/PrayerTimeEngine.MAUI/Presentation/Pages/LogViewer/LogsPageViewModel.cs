using System.Windows.Input;
using MetroLog.Maui;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Presentation.Localization;
using PrayerTimeEngine.Presentation.Pages.Settings.SettingsHandler;
using PrayerTimeEngine.Presentation.Services;
using PrayerTimeEngine.Services.Logging;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.Pages.LogViewer;

/// <summary>A level that can be ticked to show its entries, with the number of entries of that level.</summary>
[AddINotifyPropertyChangedInterface]
public class LogLevelFilterOption(LogLevel level, Action onChanged)
{
    public LogLevel Level { get; } = level;

    [OnChangedMethod(nameof(OnIsSelectedChanged))]
    public bool IsSelected { get; set; } = true;

    public int Count { get; set; }

    public string DisplayText => $"{LogsPageViewModel.GetLevelName(Level)} ({Count})";

    private void OnIsSelectedChanged() => onChanged();
}

[AddINotifyPropertyChangedInterface]
public class LogsPageViewModel(
        InMemoryLogStore logStore,
        ToastMessageService toastMessageService,
        ILogger<LogsPageViewModel> logger
    ) : CustomBaseViewModel
{
    private static readonly LogLevel[] s_levels =
        [LogLevel.Trace, LogLevel.Debug, LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Critical];

    private List<LogEntry> _allEntries = [];
    private CancellationTokenSource? _searchDebounceTokenSource;

    public List<LogLevelFilterOption> LevelFilterOptions { get; private set; } = [];

    [OnChangedMethod(nameof(OnSearchTextChanged))]
    public string SearchText { get; set; } = string.Empty;

    /// <summary>The shown entries, the newest first.</summary>
    public List<LogEntry> FilteredEntries { get; private set; } = [];

    public string StatusText { get; private set; } = string.Empty;

    #region minimum level

    /// <summary>The selectable values of the app wide level (the names shown in the picker).</summary>
    public List<string> MinimumLevelOptions { get; } =
        [.. s_levels.Append(LogLevel.None).Select(GetMinimumLevelOptionText)];

    /// <summary>The app wide level from which on entries are written at all (also into the log files).</summary>
    public string SelectedMinimumLevelOption
    {
        get => GetMinimumLevelOptionText(AppLogLevel.MinimumLevel);
        set
        {
            int index = MinimumLevelOptions.IndexOf(value);
            if (index < 0)
            {
                return;
            }

            LogLevel newLevel = index < s_levels.Length ? s_levels[index] : LogLevel.None;
            if (newLevel == AppLogLevel.MinimumLevel)
            {
                return;
            }

            // logged before the change, so that the change is visible even when the new level hides it
            logger.LogInformation("App wide log level changed from {OldLevel} to {NewLevel}", AppLogLevel.MinimumLevel, newLevel);
            AppLogLevel.MinimumLevel = newLevel;
        }
    }

    private static string GetMinimumLevelOptionText(LogLevel level) =>
        level == LogLevel.None
            ? AppResources.Logs_MinimumLevel_None
            : string.Format(AppResources.Logs_MinimumLevel_FromLevel, GetLevelName(level));

    #endregion minimum level

    public static string GetLevelName(LogLevel level) => level switch
    {
        LogLevel.Information => "Info",
        LogLevel.Critical => "Critical",
        _ => level.ToString(),
    };

    public override void Initialize(params object[] parameter)
    {
        LevelFilterOptions = [.. s_levels.Select(level => new LogLevelFilterOption(level, ApplyFilter))];
        Reload();
    }

    #region commands

    public ICommand ReloadCommand => new Command(Reload);

    public ICommand SelectAllLevelsCommand => new Command(() => SetAllLevels(true));

    public ICommand ClearCommand => new Command(() =>
    {
        logStore.Clear();
        Reload();
    });

    /// <summary>Shares the log files of the last days as a zip file (as the previous logs page did).</summary>
    public ICommand ShareLogFilesCommand => new Command(async () =>
    {
        try
        {
            using MemoryStream compressedLogs = await new LogController().GetCompressedLogs();

            string filePath = PrepareShareFilePath($"PrayerTimeEngine_Logs_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
            await File.WriteAllBytesAsync(filePath, compressedLogs.ToArray());

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = AppResources.Logs_ShareLogFiles,
                File = new ShareFile(filePath),
            });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while sharing the log files");
            toastMessageService.ShowError(exception.Message);
        }
    });

    /// <summary>Shares the currently shown (filtered) entries as a text file.</summary>
    public ICommand ShareFilteredEntriesCommand => new Command(async () =>
    {
        try
        {
            string text = string.Join(
                Environment.NewLine + Environment.NewLine,
                FilteredEntries.AsEnumerable().Reverse().Select(x => x.FullText));

            string filePath = PrepareShareFilePath($"PrayerTimeEngine_Logs_Filtered_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            await File.WriteAllTextAsync(filePath, text);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = AppResources.Logs_ShareFilteredEntries,
                File = new ShareFile(filePath),
            });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while sharing the filtered log entries");
            toastMessageService.ShowError(exception.Message);
        }
    });

    /// <summary>
    /// The path for a new file to share. The files shared before are deleted first, so that they don't pile up
    /// in the cache over time; only the last shared file remains until the next share.
    /// </summary>
    private string PrepareShareFilePath(string fileName)
    {
        string directoryPath = Path.Combine(FileSystem.CacheDirectory, "SharedLogs");

        // version 0.7.0 put the shared files directly into the cache directory
        IEnumerable<string> oldFilePaths = Directory.GetFiles(FileSystem.CacheDirectory, "PrayerTimeEngine_Logs_*");
        if (Directory.Exists(directoryPath))
        {
            oldFilePaths = oldFilePaths.Concat(Directory.GetFiles(directoryPath));
        }

        foreach (string oldFilePath in oldFilePaths)
        {
            try
            {
                File.Delete(oldFilePath);
            }
            catch (Exception exception)
            {
                // e.g. still opened by the app it was shared with: deleted on one of the next shares instead
                logger.LogWarning(exception, "Old shared log file {FilePath} could not be deleted", oldFilePath);
            }
        }

        Directory.CreateDirectory(directoryPath);
        return Path.Combine(directoryPath, fileName);
    }

    #endregion commands

    public void Reload()
    {
        _allEntries = logStore.GetEntries();

        var countByLevel = _allEntries.GroupBy(x => x.Level).ToDictionary(x => x.Key, x => x.Count());
        foreach (LogLevelFilterOption option in LevelFilterOptions)
        {
            option.Count = countByLevel.GetValueOrDefault(option.Level);
        }

        ApplyFilter();
    }

    private void SetAllLevels(bool isSelected)
    {
        foreach (LogLevelFilterOption option in LevelFilterOptions)
        {
            option.IsSelected = isSelected;
        }

        ApplyFilter();
    }

    private void OnSearchTextChanged()
    {
        // filtering thousands of entries on every keystroke would make typing sluggish
        _searchDebounceTokenSource?.Cancel();
        _searchDebounceTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = _searchDebounceTokenSource.Token;

        Task.Run(async () =>
        {
            await Task.Delay(250, cancellationToken);

            // the shown list is bound to the UI
            MainThread.BeginInvokeOnMainThread(ApplyFilter);
        }, cancellationToken);
    }

    private void ApplyFilter()
    {
        var selectedLevels = LevelFilterOptions.Where(x => x.IsSelected).Select(x => x.Level).ToHashSet();
        string[] searchTerms = (SearchText ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // every search term has to occur somewhere in the entry (in any order)
        FilteredEntries =
            [.. _allEntries
                .Where(x => selectedLevels.Contains(x.Level))
                .Where(x => searchTerms.All(term => x.FullText.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .Reverse()];

        StatusText = string.Format(AppResources.Logs_Status, FilteredEntries.Count, _allEntries.Count, InMemoryLogStore.CAPACITY);
    }
}
