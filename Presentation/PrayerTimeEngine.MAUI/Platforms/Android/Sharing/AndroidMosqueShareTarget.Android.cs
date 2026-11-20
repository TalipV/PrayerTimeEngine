using Android.Content;
using Android.Content.PM;
using PrayerTimeEngine.Presentation.Services.MosqueSharing;

namespace PrayerTimeEngine.Platforms.Android.Sharing;

/// <summary>
/// Switches <see cref="MosqueShareReceiverActivity"/> on and off, so that the app is only offered in share menus
/// while the dialog for a new mosque profile is open. Registered as a singleton.
/// </summary>
public sealed class AndroidMosqueShareTarget : IMosqueShareTarget
{
    private readonly Lock _lock = new();
    private string? _pendingText;

    public bool IsSupported => true;

    public event EventHandler<string>? TextReceived;

    public void Activate()
    {
        SetReceiverEnabled(global::Android.App.Application.Context, enabled: true);
    }

    public void Deactivate()
    {
        SetReceiverEnabled(global::Android.App.Application.Context, enabled: false);
    }

    public bool TryTakePendingText(out string text)
    {
        lock (_lock)
        {
            text = _pendingText ?? "";
            _pendingText = null;
        }

        return text.Length > 0;
    }

    /// <summary>Called by <see cref="MosqueShareReceiverActivity"/> with the shared text.</summary>
    internal void Receive(string text)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            EventHandler<string>? handler = TextReceived;

            if (handler is null)
            {
                // nobody listens (the app was restarted for the share): keep it for the next dialog
                lock (_lock)
                {
                    _pendingText = text;
                }

                return;
            }

            handler(this, text);
        });
    }

    /// <summary>
    /// Also called on every app start: if the system closed the app while the dialog was open, the share entry
    /// would otherwise stay in every share menu until the dialog is opened and closed again.
    /// </summary>
    internal static void SetReceiverEnabled(Context context, bool enabled)
    {
        var component = new ComponentName(context, Java.Lang.Class.FromType(typeof(MosqueShareReceiverActivity)));

        context.PackageManager?.SetComponentEnabledSetting(
            component,
            enabled ? ComponentEnabledState.Enabled : ComponentEnabledState.Default,
            ComponentEnableOption.DontKillApp);
    }
}
