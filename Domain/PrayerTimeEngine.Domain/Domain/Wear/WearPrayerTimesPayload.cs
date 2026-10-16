using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.PrayerTimeProgress;

namespace PrayerTimeEngine.Core.Domain.Wear;

/// <summary>
/// The prayer times the phone hands to the watch. The watch derives everything it shows from these,
/// including the countdown, so it is only sent again when the times themselves change.
/// <para>
/// Times travel as unix seconds plus the time zone id, the watch rebuilds the zoned times from them.
/// </para>
/// </summary>
public sealed record WearPrayerTimesPayload(
    string ProfileName,
    string City,
    string TimeZoneId,
    List<WearPrayerTimeWindowDto> Windows)
{
    public const string DATA_PATH = "/prayer-times";
    public const string PAYLOAD_KEY = "payload";
    public const string GENERATED_AT_KEY = "generatedAt";

    public static WearPrayerTimesPayload Create(
        string profileName,
        string city,
        DateTimeZone timeZone,
        IEnumerable<PrayerTimeWindow> windows)
    {
        return new WearPrayerTimesPayload(
            profileName,
            city,
            timeZone.Id,
            [.. windows.Select(window => new WearPrayerTimeWindowDto(
                window.Section,
                ToUnixSeconds(window.Start),
                ToUnixSeconds(window.End),
                [.. window.SubTimes.Select(subTime => new WearSubTimeDto(subTime.Name, ToUnixSeconds(subTime.Time)))]))]);
    }

    /// <summary>
    /// Rebuilds the windows in the time zone they were calculated for. Falls back to UTC when the
    /// watch does not know that zone, which only shifts the displayed clock times, never the progress.
    /// </summary>
    public List<PrayerTimeWindow> ToWindows()
    {
        DateTimeZone timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(TimeZoneId) ?? DateTimeZone.Utc;

        return [.. Windows.Select(window => new PrayerTimeWindow(
            window.Section,
            FromUnixSeconds(window.StartUnixSeconds, timeZone),
            FromUnixSeconds(window.EndUnixSeconds, timeZone),
            [.. window.SubTimes.Select(subTime => new PrayerTimeSubTime(subTime.Name, FromUnixSeconds(subTime.TimeUnixSeconds, timeZone)))]))];
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, WearPrayerTimesJsonContext.Default.WearPrayerTimesPayload);
    }

    public static WearPrayerTimesPayload? FromJson(string json)
    {
        return JsonSerializer.Deserialize(json, WearPrayerTimesJsonContext.Default.WearPrayerTimesPayload);
    }

    private static long? ToUnixSeconds(ZonedDateTime? time)
    {
        return time?.ToInstant().ToUnixTimeSeconds();
    }

    private static ZonedDateTime? FromUnixSeconds(long? unixSeconds, DateTimeZone timeZone)
    {
        return unixSeconds is null
            ? null
            : Instant.FromUnixTimeSeconds(unixSeconds.Value).InZone(timeZone);
    }
}

public sealed record WearPrayerTimeWindowDto(
    ETimeSection Section,
    long? StartUnixSeconds,
    long? EndUnixSeconds,
    List<WearSubTimeDto> SubTimes);

public sealed record WearSubTimeDto(string Name, long? TimeUnixSeconds);

/// <summary>Source generated, so the (trimmed) watch app needs no reflection to read the payload.</summary>
[JsonSerializable(typeof(WearPrayerTimesPayload))]
internal sealed partial class WearPrayerTimesJsonContext : JsonSerializerContext;
