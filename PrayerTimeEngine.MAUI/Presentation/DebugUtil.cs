using OnScreenSizeMarkup.Maui;
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
    }
}
