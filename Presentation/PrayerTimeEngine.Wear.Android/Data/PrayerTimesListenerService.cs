using Android.Gms.Wearable;
using Android.Runtime;
using Android.Util;
using PrayerTimeEngine.Core.Domain.Wear;

namespace PrayerTimeEngine.Wear.Data;

/// <summary>Receives the prayer times whenever the phone puts new ones into the data layer.</summary>
[Service(Exported = true, Name = "com.companyname.prayertimeengine.wear.PrayerTimesListenerService")]
[IntentFilter(
    ["com.google.android.gms.wearable.DATA_CHANGED"],
    DataScheme = "wear",
    DataHost = "*",
    DataPathPrefix = WearPrayerTimesPayload.DATA_PATH)]
public class PrayerTimesListenerService : WearableListenerService
{
    public override void OnDataChanged(DataEventBuffer dataEvents)
    {
        for (int i = 0; i < dataEvents.Count; i++)
        {
            IDataEvent dataEvent = dataEvents.Get(i)!.JavaCast<IDataEvent>()!;

            if (dataEvent.Type != DataEvent.TypeChanged
                || dataEvent.DataItem.Uri.Path != WearPrayerTimesPayload.DATA_PATH)
            {
                continue;
            }

            string? json = DataMapItem.FromDataItem(dataEvent.DataItem).DataMap.GetString(WearPrayerTimesPayload.PAYLOAD_KEY);

            if (string.IsNullOrEmpty(json))
            {
                continue;
            }

            Log.Info("PrayerTimeEngine.Wear", "Received new prayer times from the phone");
            PrayerTimesStore.Save(this, json);
        }
    }
}
