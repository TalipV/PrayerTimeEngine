using PrayerTimeEngine.Core.Common;

namespace PrayerTimeEngine.Services;

internal class PreferenceService : IPreferenceService
{
    // TODO less ugly
    private const string DO_RESET_KEY = "DB_RESET_KEY";
    public void SetDoReset()
    {
        SetValue(DO_RESET_KEY, "JA");

        // setting value didn't work without this on my physical smartphone
        Thread.Sleep(100);
    }
    public bool CheckAndResetDoReset()
    {
        bool doReset = GetValue(DO_RESET_KEY, "NEIN") == "JA";
        SetValue(DO_RESET_KEY, "NEIN");
        return doReset;
    }

    public string GetValue(string key, string defaultValue)
    {
        return Preferences.Get(key, defaultValue);
    }

    public void SetValue(string key, string value)
    {
        Preferences.Set(key, value);
    }
}
