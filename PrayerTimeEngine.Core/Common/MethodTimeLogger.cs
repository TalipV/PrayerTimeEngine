using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;

namespace PrayerTimeEngine.Core.Common
{
    public static class MethodTimeLogger
    {
        public static ILogger logger;

        // temporary solution of course
        public static readonly ConcurrentBag<string> _notLoggedStuff = new ();

        public static void Log(MethodBase methodBase, TimeSpan timeSpan, string message)
        {
            if (logger == null)
            {
                _notLoggedStuff.Add(
                    $"TIME-LOGGER: {Environment.NewLine}{methodBase.DeclaringType}.{methodBase.Name}, {Environment.NewLine}{timeSpan.TotalMilliseconds:N0} ms");
                return;
            }

            foreach (var item in _notLoggedStuff.Reverse())
                logger.LogInformation(item);

            _notLoggedStuff.Clear();

            logger?.LogInformation(
                "TIME-LOGGER: \r\n{DeclaringType}.{MethodName}, \r\n{Milliseconds} ms",
                methodBase.DeclaringType,
                methodBase.Name,
                timeSpan.TotalMilliseconds.ToString("N0"));
        }
    }
}
