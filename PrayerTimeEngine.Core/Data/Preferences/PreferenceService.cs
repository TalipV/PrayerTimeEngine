using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Data.Preferences
{
    public class PreferenceService(IPreferenceAccess preferenceAccess)
    {
        private const string CURRENT_PROFILE_KEY_PREFIX = "Profile";
        private const string PRAYER_TIMES_KEY_PREFIX = "PrayerTimes_";

        private static readonly JsonSerializerOptions _settings = new
            JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true
            };

        public void SaveCurrentData(Profile profile, PrayerTimesBundle prayerTimesBundle)
        {
            string jsonData = JsonSerializer.Serialize(prayerTimesBundle, _settings);
            preferenceAccess.SetValue(getPrayerPreferenceKey(profile), jsonData);

            preferenceAccess.SetValue(CURRENT_PROFILE_KEY_PREFIX, JsonSerializer.Serialize(profile, _settings));
        }

        public PrayerTimesBundle GetCurrentData(Profile profile)
        {
            string key = getPrayerPreferenceKey(profile);
            string jsonData = preferenceAccess.GetValue(key, string.Empty);

            if (string.IsNullOrEmpty(jsonData))
                return null;

            return JsonSerializer.Deserialize<PrayerTimesBundle>(jsonData, _settings);
        }
        public Profile GetCurrentProfile()
        {
            string key = CURRENT_PROFILE_KEY_PREFIX;
            string jsonData = preferenceAccess.GetValue(key, string.Empty);

            if (string.IsNullOrEmpty(jsonData))
                return null;

            return JsonSerializer.Deserialize<Profile>(jsonData, _settings);
        }

        private string getPrayerPreferenceKey(Profile profile)
        {
            return PRAYER_TIMES_KEY_PREFIX + profile.ID;
        }
    }
}
