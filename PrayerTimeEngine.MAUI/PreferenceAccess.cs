using PrayerTimeEngine.Core.Data.Preferences;

namespace PrayerTimeEngine
{
    public class PreferenceAccess : IPreferenceAccess
    {
        public string GetValue(string key, string defaultValue)
        {
            return Preferences.Default.Get(key, defaultValue);
        }

        public void SetValue(string key, string value)
        {
            Preferences.Default.Set(key, value);
        }

        public void RemoveValue(string key)
        {
            Preferences.Default.Remove(key);
        }
    }
}
