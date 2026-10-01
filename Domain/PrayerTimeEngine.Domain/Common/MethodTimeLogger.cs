using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace PrayerTimeEngine.Core.Common;

public static class MethodTimeLogger
{
    public static ILogger Logger { get; set; }

    // temporary solution of course
    public static readonly ConcurrentBag<string> NotLoggedStuff = [];

#pragma warning disable IDE0060 // Remove unused parameter
    public static void Log(MethodBase methodBase, TimeSpan timeSpan, string message)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        if (Logger is null)
        {
            NotLoggedStuff.Add(
                $"TIME-LOGGER: {methodBase.DeclaringType}.{methodBase.Name}, {timeSpan.TotalMilliseconds:N0} ms");
            return;
        }

        ExecuteMissedOutLogs();

        Logger.LogInformation(
            "TIME-LOGGER: {DeclaringType}.{MethodName}, {Milliseconds} ms",
            methodBase.DeclaringType,
            methodBase.Name,
            timeSpan.TotalMilliseconds.ToString("N0"));
    }

    private static void ExecuteMissedOutLogs()
    {
        foreach (string? notLoggedMessage in NotLoggedStuff.Reverse())
        {
            Logger.LogInformation("{Message}", notLoggedMessage);
        }

        NotLoggedStuff.Clear();
    }
}
