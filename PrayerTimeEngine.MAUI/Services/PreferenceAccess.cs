using PrayerTimeEngine.Core.Data.PreferenceManager;

namespace PrayerTimeEngine.Services
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
