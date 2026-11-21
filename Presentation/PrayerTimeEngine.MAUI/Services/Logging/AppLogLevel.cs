using Microsoft.Extensions.Logging;

namespace PrayerTimeEngine.Services.Logging;

/// <summary>
/// The level from which on log entries are written at all (into the logs page and the log files).
/// Can be changed on the logs page while the app is running and is kept across app starts.
/// </summary>
public static class AppLogLevel
{
    private const string PREFERENCE_KEY = "LOG_MINIMUM_LEVEL";

    /// <summary>Everything is logged unless the user chose otherwise (as before this setting existed).</summary>
    public const LogLevel DEFAULT_MINIMUM_LEVEL = LogLevel.Trace;

    private static LogLevel? s_minimumLevel;

    public static LogLevel MinimumLevel
    {
        get => s_minimumLevel ??= Load();
        set
        {
            s_minimumLevel = value;
            Preferences.Default.Set(PREFERENCE_KEY, value.ToString());
        }
    }

    private static LogLevel Load()
    {
        string storedValue = Preferences.Default.Get(PREFERENCE_KEY, DEFAULT_MINIMUM_LEVEL.ToString());
        return Enum.TryParse(storedValue, out LogLevel level) ? level : DEFAULT_MINIMUM_LEVEL;
    }
}
