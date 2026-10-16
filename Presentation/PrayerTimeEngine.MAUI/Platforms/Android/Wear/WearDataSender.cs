using Android.Content;
using Android.Gms.Extensions;
using Android.Gms.Wearable;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Core.Domain.Wear;

namespace PrayerTimeEngine.Platforms.Android.Wear;

/// <summary>
/// Hands the prayer times to the watch app through the Wear OS data layer. The data layer keeps the
/// latest item and delivers it whenever the watch is reachable, so a send never needs to be retried.
/// </summary>
internal static class WearDataSender
{
    private static string? s_lastSentJson;

    /// <summary>Never throws: the watch is an extra, a failure must not affect the phone notification.</summary>
    public static async Task SendAsync(Context context, WearPrayerTimesPayload payload, ILogger logger)
    {
        try
        {
            string json = payload.ToJson();

            // the data layer only notifies the watch about changed items anyway, but skipping here
            // saves the whole round trip for the identical payload checked every minute
            if (json == s_lastSentJson)
            {
                return;
            }

            var dataMapRequest = PutDataMapRequest.Create(WearPrayerTimesPayload.DATA_PATH);
            dataMapRequest.DataMap.PutString(WearPrayerTimesPayload.PAYLOAD_KEY, json);
            dataMapRequest.DataMap.PutLong(WearPrayerTimesPayload.GENERATED_AT_KEY, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            PutDataRequest request = dataMapRequest.AsPutDataRequest().SetUrgent();

            await WearableClass.GetDataClient(context).PutDataItem(request).AsAsync<IDataItem>();

            s_lastSentJson = json;
            logger.LogInformation("Sent prayer times of profile {ProfileName} to the watch", payload.ProfileName);
        }
        catch (Exception exception)
        {
            // e.g. no Google Play services or no paired watch at all
            logger.LogWarning(exception, "Prayer times could not be sent to the watch");
        }
    }
}
