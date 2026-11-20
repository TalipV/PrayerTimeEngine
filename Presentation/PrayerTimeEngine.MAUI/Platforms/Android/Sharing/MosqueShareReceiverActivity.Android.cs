using Android.App;
using Android.Content;
using Android.OS;
using PrayerTimeEngine.Presentation.Services.MosqueSharing;

namespace PrayerTimeEngine.Platforms.Android.Sharing;

/// <summary>
/// The entry in the share menu of other apps ("Share" in the Mawaqit app). It is disabled in the manifest
/// (<c>Enabled = false</c>) and only switched on by <see cref="AndroidMosqueShareTarget"/> while the dialog for
/// a new mosque profile is open, so that the app does not show up when texts from any other app are shared.
/// It hands the text over, brings the app back to the front and closes itself without ever being visible.
/// </summary>
[Activity(
    Label = "@string/mosque_share_target_label",
    Icon = "@mipmap/appicon",
    Theme = "@android:style/Theme.Translucent.NoTitleBar",
    Enabled = false,
    Exported = true,
    NoHistory = true,
    ExcludeFromRecents = true)]
[IntentFilter(
    [Intent.ActionSend],
    Categories = [Intent.CategoryDefault],
    DataMimeType = "text/plain")]
public class MosqueShareReceiverActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        string sharedText = string.Join(
            "\n",
            new[]
            {
                Intent?.GetStringExtra(Intent.ExtraSubject),
                Intent?.GetStringExtra(Intent.ExtraText)
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

        if (!string.IsNullOrWhiteSpace(sharedText)
            && MauiProgram.ServiceProvider.GetService<IMosqueShareTarget>() is AndroidMosqueShareTarget shareTarget)
        {
            shareTarget.Receive(sharedText);
        }

        // brings the running app to the front, exactly like tapping its icon on the home screen
        // (or starts it, if the system had closed it in the meantime)
        Intent? launchIntent = PackageManager?.GetLaunchIntentForPackage(PackageName!);
        if (launchIntent is not null)
        {
            launchIntent.AddFlags(ActivityFlags.NewTask);
            StartActivity(launchIntent);
        }

        Finish();
    }
}
