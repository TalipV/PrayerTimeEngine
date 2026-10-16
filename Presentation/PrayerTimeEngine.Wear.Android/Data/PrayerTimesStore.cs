using Android.Content;
using Android.Gms.Extensions;
using Android.Gms.Wearable;
using Android.Runtime;
using Android.Util;
using AndroidX.Wear.Tiles;
using AndroidX.Wear.WatchFace.Complications.Datasource;
using PrayerTimeEngine.Core.Domain.Wear;
using PrayerTimeEngine.Wear.Complications;
using PrayerTimeEngine.Wear.Tiles;

namespace PrayerTimeEngine.Wear.Data;

/// <summary>
/// Keeps the last prayer times received from the phone, so tile and complication work without a
/// connection to it. Everything shown on the watch is derived from these times locally.
/// </summary>
internal static class PrayerTimesStore
{
    private const string LOG_TAG = "PrayerTimeEngine.Wear";
    private const string PREFERENCES_NAME = "prayer_times";

    public static WearPrayerTimesPayload? Load(Context context)
    {
        string? json = GetPreferences(context).GetString(WearPrayerTimesPayload.PAYLOAD_KEY, null);

        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return WearPrayerTimesPayload.FromJson(json);
        }
        catch (Exception exception)
        {
            Log.Error(LOG_TAG, $"Stored prayer times could not be read: {exception}");
            return null;
        }
    }

    /// <summary>Stores the payload and makes tile and complication pick it up.</summary>
    public static void Save(Context context, string json)
    {
        GetPreferences(context).Edit()!
            .PutString(WearPrayerTimesPayload.PAYLOAD_KEY, json)!
            .Apply();

        RequestUpdates(context);
    }

    /// <summary>
    /// Fetches the item the phone put into the data layer. The listener service only hears about
    /// changes, this catches the times that were already there before the watch app was installed.
    /// </summary>
    public static async Task RefreshFromDataLayerAsync(Context context)
    {
        try
        {
            Android.Net.Uri uri = new Android.Net.Uri.Builder()
                .Scheme(PutDataRequest.WearUriScheme)!
                .Path(WearPrayerTimesPayload.DATA_PATH)!
                .Build()!;

            var dataItems = (DataItemBuffer)await WearableClass.GetDataClient(context).GetDataItems(uri);

            try
            {
                for (int i = 0; i < dataItems.Count; i++)
                {
                    IDataItem dataItem = dataItems.Get(i)!.JavaCast<IDataItem>()!;
                    string? json = DataMapItem.FromDataItem(dataItem).DataMap.GetString(WearPrayerTimesPayload.PAYLOAD_KEY);

                    if (!string.IsNullOrEmpty(json))
                    {
                        Save(context, json);
                    }
                }
            }
            finally
            {
                dataItems.Release();
            }
        }
        catch (Exception exception)
        {
            Log.Warn(LOG_TAG, $"Prayer times could not be fetched from the phone: {exception}");
        }
    }

    public static void RequestUpdates(Context context)
    {
        TileService.GetUpdater(context)!.RequestUpdate(Java.Lang.Class.FromType(typeof(PrayerTimeTileService)));

        ComplicationDataSourceUpdateRequester
            .Create(context, new ComponentName(context, Java.Lang.Class.FromType(typeof(PrayerTimeComplicationService))))
            .RequestUpdateAll();
    }

    private static ISharedPreferences GetPreferences(Context context)
    {
        return context.GetSharedPreferences(PREFERENCES_NAME, FileCreationMode.Private)!;
    }
}
