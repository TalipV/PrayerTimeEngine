using System.Diagnostics;
using System.Reflection;

namespace PrayerTimeEngine.Core.Common
{
    public static class MethodTimeLogger
    {
        public static void Log(MethodBase methodBase, TimeSpan timeSpan, string message)
        {
            Debug.WriteLine($"TIME-LOGGER: {methodBase.DeclaringType}.{methodBase.Name}, {timeSpan}");
        }
    }
}
