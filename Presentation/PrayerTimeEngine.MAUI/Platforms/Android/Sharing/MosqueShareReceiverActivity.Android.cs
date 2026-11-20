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
// Any content type, not only "text/plain": the Mawaqit app shares its text together with an image, so the
// share is announced as an image / "*/*" and a text-only filter would never be offered. Accepting everything
// is fine here, because this entry only exists while the dialog for a new mosque profile is open.
[IntentFilter(
    [Intent.ActionSend, Intent.ActionSendMultiple],
    Categories = [Intent.CategoryDefault],
    DataMimeType = "*/*")]
public class MosqueShareReceiverActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        string sharedText = GetSharedText(Intent);

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

    /// <summary>
    /// Collects the text of a share: subject and text extras (read as CharSequence, because apps may put
    /// formatted text there, which <c>GetStringExtra</c> would return as null) plus the texts of the clip data,
    /// which some apps use instead of the extras.
    /// </summary>
    private static string GetSharedText(Intent? intent)
    {
        if (intent is null)
        {
            return "";
        }

        List<string?> texts =
        [
            intent.GetCharSequenceExtra(Intent.ExtraSubject),
            intent.GetCharSequenceExtra(Intent.ExtraText),
        ];

        if (intent.ClipData is { } clipData)
        {
            for (int index = 0; index < clipData.ItemCount; index++)
            {
                texts.Add(clipData.GetItemAt(index)?.Text);
            }
        }

        return string.Join(
            "\n",
            texts
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct());
    }
}
