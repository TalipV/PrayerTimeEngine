using OnScreenSizeMarkup.Maui;
using OnScreenSizeMarkup.Maui.Helpers;
using System.Reflection;

namespace PrayerTimeEngine.Presentation
{
    public class DebugUtil
    {
        public static string GetScreenSizeCategoryName()
        {
            Type helpersType = typeof(OnScreenSizeHelpers);// Fetch screen diagonal from OnScreenSizeHelpers with no parameters
            MethodInfo getDiagonalMethod = helpersType?.GetMethod("GetScreenDiagonalInches",
                                                                 BindingFlags.Static | BindingFlags.NonPublic,
                                                                 null,
                                                                 new Type[] { },
                                                                 null);

            double screenDiagonalInches = (double)getDiagonalMethod?.Invoke(null, null);

            // 2. Fetch the Categorizer property from Manager.Current
            Type managerType = typeof(Manager);
            PropertyInfo instanceProperty = managerType?.GetProperty("Current", BindingFlags.Static | BindingFlags.Public);
            object managerInstance = instanceProperty?.GetValue(null);
            PropertyInfo categorizerProperty = managerType?.GetProperty("Categorizer", BindingFlags.Instance | BindingFlags.NonPublic);
            object categorizerInstance = categorizerProperty?.GetValue(managerInstance);

            // 3. Call GetCategoryByDiagonalSize method on Categorizer
            Type categorizerType = categorizerInstance.GetType();
            MethodInfo getCategoryMethod = categorizerType.GetMethod("GetCategoryByDiagonalSize");
            object[] parameters = new object[] { Manager.Current.Mappings, screenDiagonalInches };
            string result = getCategoryMethod?.Invoke(categorizerInstance, parameters)?.ToString() ?? "NOT FOUND";

            return result;
        }
    }
}
