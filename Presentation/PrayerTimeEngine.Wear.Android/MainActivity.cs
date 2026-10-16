using Android.Views;
using NodaTime;
using PrayerTimeEngine.Core.Domain.PrayerTimeProgress;
using PrayerTimeEngine.Core.Domain.Wear;
using PrayerTimeEngine.Wear.Data;

namespace PrayerTimeEngine.Wear;

/// <summary>
/// Lists the prayer times of today as received from the phone. Mainly a way to see whether the
/// watch got any times at all, the tile and the complication are what is meant for every day.
/// </summary>
[Activity(
    Label = "PrayerTimeEngine",
    Name = JAVA_NAME,
    MainLauncher = true,
    Exported = true,
    Theme = "@android:style/Theme.DeviceDefault")]
public class MainActivity : Activity
{
    public const string JAVA_NAME = "com.companyname.prayertimeengine.wear.MainActivity";

    private LinearLayout? _content;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        int padding = (int)(24 * Resources!.DisplayMetrics!.Density);

        _content = new LinearLayout(this) { Orientation = Orientation.Vertical };
        _content.SetPadding(padding, padding, padding, padding);

        var scrollView = new ScrollView(this);
        scrollView.AddView(_content);
        SetContentView(scrollView);

        Render();
    }

    protected override async void OnResume()
    {
        base.OnResume();

        await PrayerTimesStore.RefreshFromDataLayerAsync(this);
        Render();
    }

    private void Render()
    {
        if (_content is null)
        {
            return;
        }

        _content.RemoveAllViews();

        WearPrayerTimesPayload? payload = PrayerTimesStore.Load(this);

        if (payload is null)
        {
            AddLine("Noch keine Gebetszeiten.\nÖffne die App auf dem Handy.", 14f);
            return;
        }

        AddLine(payload.City, 16f);
        AddLine(payload.ProfileName, 12f);

        List<PrayerTimeWindow> windows = payload.ToWindows();
        LocalDate today = SystemClock.Instance.GetCurrentInstant().InZone(windows.FirstOrDefault()?.Start?.Zone ?? DateTimeZone.Utc).Date;

        foreach (PrayerTimeWindow window in windows.Where(window => window.Start?.Date == today))
        {
            AddLine($"{window.Section}  {window.Start:HH:mm} – {window.End:HH:mm}", 14f);
        }
    }

    private void AddLine(string text, float size)
    {
        _content!.AddView(new TextView(this)
        {
            Text = text,
            TextSize = size,
            Gravity = GravityFlags.CenterHorizontal,
        });
    }
}
