namespace PrayerTimeEngine.Presentation;

/// <summary>
/// Central palette for the whole app.
/// <para>
/// A neutral graphite dark theme with cream text: the surfaces carry no hue of their own, so the
/// only colour on screen is the amber <see cref="Accent"/> (taken from the sun in the app icon) and
/// the red of the current time. The surfaces are layered from dark to light: <see cref="Background"/>,
/// <see cref="Card"/>, <see cref="Surface"/>.
/// </para>
/// </summary>
public static class AppColors
{
    /// <summary>Page background. Same value as the splash screen background.</summary>
    public static readonly Color Background = Color.FromArgb("#101114");

    /// <summary>
    /// Cards on <see cref="Background"/>, e.g. one prayer on the main page. Only a small step above
    /// <see cref="Background"/>, so many cards next to each other still look calm.
    /// </summary>
    public static readonly Color Card = Color.FromArgb("#18191D");

    /// <summary>Raised elements, e.g. the rows of the options menu, which sit on a card themselves.</summary>
    public static readonly Color Surface = Color.FromArgb("#222328");

    /// <summary>Borders and separators.</summary>
    public static readonly Color Border = Color.FromArgb("#34363D");

    /// <summary>
    /// All primary text, from prayer names down to times.
    /// <para>
    /// A cream white instead of pure white, which would look cold and harsh on the near black
    /// background; the layout already separates the levels through size and weight.
    /// </para>
    /// </summary>
    public static readonly Color Text = Color.FromArgb("#F3E9D6");

    /// <summary>Labels next to a value, e.g. the name of a sub time. <see cref="Text"/> pushed towards the background.</summary>
    public static readonly Color TextMuted = Color.FromArgb("#8D8F97");

    /// <summary>Body of the prayer time graphic. Lighter than <see cref="Card"/>, so the boxes drawn on it stand out.</summary>
    public static readonly Color GraphicSurface = Color.FromArgb("#26272C");

    /// <summary>Accent, taken from the sun in the app icon.</summary>
    public static readonly Color Accent = Color.FromArgb("#F0B44C");

    /// <summary>
    /// <see cref="Accent"/> as a packed ARGB integer.
    /// <para>
    /// Native platform APIs take colours as an int instead of a <see cref="Color"/>. The Android
    /// notification accent is one of them: a notification small icon is alpha-only, and the system
    /// paints it in the colour handed to <c>Notification.Builder.SetColor</c>.
    /// </para>
    /// </summary>
    public static int AccentArgb => Accent.ToInt();

    /// <summary>Current time indicator and destructive actions. A light red, so it stays legible on the dark background.</summary>
    public static readonly Color CurrentTime = Color.FromArgb("#FF6B5E");
}
