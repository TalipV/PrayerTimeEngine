namespace PrayerTimeEngine.Core.Common
{
    public interface IPreferenceService
    {
        void SetDoReset();
        bool CheckAndResetDoReset();

        string GetValue(string key, string defaultValue);
        void SetValue(string key, string value);
    }
}
