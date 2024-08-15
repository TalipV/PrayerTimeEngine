using OnScreenSizeMarkup.Maui.Helpers;
using OnScreenSizeMarkup.Maui.Providers;
using System.Reflection;

namespace PrayerTimeEngine.Presentation
{
    public class DebugUtil
    {
        public static string GetScreenSizeCategoryName()
        {
            Type helpersType = typeof(OnScreenSizeHelpers);// Fetch screen diagonal from OnScreenSizeHelpers with no parameters

            FieldInfo screenCategoryProviderFieldInfo = helpersType
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(x => x.Name == "screenCategoryProvider");

            IScreenCategoryProvider screenCategoryProvider = screenCategoryProviderFieldInfo.GetValue(OnScreenSizeHelpers.Instance) as IScreenCategoryProvider;

            return screenCategoryProvider?.GetCategory().ToString() ?? "NOT FOUND";
        }

        private const string SIZE_VALUES_PREFERENCE_KEY = "SIZE_VALUE_";

        public static int[] GetSizeValues(int defaultValue)
        {
            string sizeTextValues = Preferences.Get(SIZE_VALUES_PREFERENCE_KEY, null);

            if (string.IsNullOrWhiteSpace(sizeTextValues))
            {
                return [defaultValue, defaultValue, defaultValue, defaultValue, defaultValue];
            }

            return sizeTextValues.Split(",").Select(int.Parse).ToArray();
        }
        public static void SetSizeValue(int[] sizeValues)
        {
            if (sizeValues.Length != 5 || sizeValues.Any(x => x < 6))
            {
                Preferences.Remove(SIZE_VALUES_PREFERENCE_KEY);
            }
            else
            {
                Preferences.Set(SIZE_VALUES_PREFERENCE_KEY, string.Join(",", sizeValues));
            }
        }

        public static string GenerateDebugID()
        {
            return Guid.NewGuid().ToString().Substring(0, 5);
        }
    }
}
