namespace PrayerTimeEngine.Core.Data.Preferences
{
    public interface IPreferenceAccess
    {
        public void SetValue(string key, string value);
        public string GetValue(string key, string defaultValue);
        public void RemoveValue(string key);
    }
}
