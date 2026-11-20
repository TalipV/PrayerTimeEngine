namespace PrayerTimeEngine.Presentation.Services.MosqueSharing;

/// <summary>
/// Lets other apps (the Mawaqit app's "Share" button) send a mosque to this app, but only for as long as the
/// dialog for a new mosque profile is open. The rest of the time the app does not appear in any share menu,
/// because the system cannot restrict a share target to texts from one particular app.
/// </summary>
public interface IMosqueShareTarget
{
    /// <summary>Whether this platform can offer the app as a share target at all.</summary>
    bool IsSupported { get; }

    /// <summary>Raised on the main thread with the shared text while the share target is active.</summary>
    event EventHandler<string>? TextReceived;

    /// <summary>Shows the app in the share menus of other apps.</summary>
    void Activate();

    /// <summary>Removes the app from the share menus of other apps again.</summary>
    void Deactivate();

    /// <summary>
    /// Returns a text that was shared while nobody listened (e.g. when the system had closed the app in the
    /// meantime and restarted it for the share), so that the next dialog can pick it up.
    /// </summary>
    bool TryTakePendingText(out string text);
}

/// <summary>For platforms without a share target.</summary>
public sealed class NoOpMosqueShareTarget : IMosqueShareTarget
{
    public bool IsSupported => false;

    public event EventHandler<string>? TextReceived
    {
        add { }
        remove { }
    }

    public void Activate()
    {
    }

    public void Deactivate()
    {
    }

    public bool TryTakePendingText(out string text)
    {
        text = "";
        return false;
    }
}
