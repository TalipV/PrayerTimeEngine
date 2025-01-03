using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;

namespace PrayerTimeEngine.Core.Common;

public static class MethodTimeLogger
{
    public static ILogger logger;

    // temporary solution of course
    public static readonly ConcurrentBag<string> _notLoggedStuff = [];

#pragma warning disable IDE0060 // Remove unused parameter
    public static void Log(MethodBase methodBase, TimeSpan timeSpan, string message)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        if (logger is null)
        {
            _notLoggedStuff.Add(
                $"TIME-LOGGER: {Environment.NewLine}{methodBase.DeclaringType}.{methodBase.Name}, {Environment.NewLine}{timeSpan.TotalMilliseconds:N0} ms");
            return;
        }

        foreach (var notLoggedMessage in _notLoggedStuff.Reverse())
            logger.LogInformation("{Message}", notLoggedMessage);

        _notLoggedStuff.Clear();

        logger?.LogInformation(
            "TIME-LOGGER: \r\n{DeclaringType}.{MethodName}, \r\n{Milliseconds} ms",
            methodBase.DeclaringType,
            methodBase.Name,
            timeSpan.TotalMilliseconds.ToString("N0"));
    }
}
