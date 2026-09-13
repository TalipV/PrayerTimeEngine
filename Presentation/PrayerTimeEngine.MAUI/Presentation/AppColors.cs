namespace PrayerTimeEngine.Presentation;

/// <summary>
/// Central palette for the whole app.
/// <para>
/// The values are taken from the app icon and the splash screen, so the app keeps the same
/// night-sky look from the moment it is launched.
/// </para>
/// </summary>
public static class AppColors
{
    /// <summary>Page background. Same value as the splash screen background.</summary>
    public static readonly Color Background = Color.FromArgb("#0B1524");

    /// <summary>Slightly lifted background for elements that sit on top of <see cref="Background"/>.</summary>
    public static readonly Color Surface = Color.FromArgb("#16294A");

    /// <summary>Borders and separators on <see cref="Background"/>.</summary>
    public static readonly Color Border = Color.FromArgb("#4C6087");

    /// <summary>
    /// All text on <see cref="Background"/>, from prayer names down to sub times.
    /// <para>
    /// Deliberately one single tone instead of a primary/secondary pair: pure white is harsh on
    /// the dark blue, and the layout already separates the levels through size and weight.
    /// </para>
    /// </summary>
    public static readonly Color Text = Color.FromArgb("#B8C4DA");

    /// <summary>Body of the prayer time graphic. Dark grey, so it separates from the blue <see cref="Background"/> without competing with it.</summary>
    public static readonly Color GraphicSurface = Color.FromArgb("#2A3140");

    /// <summary>Accent, taken from the sun in the app icon.</summary>
    public static readonly Color Accent = Color.FromArgb("#EFB03E");

    /// <summary>
    /// <see cref="Accent"/> as a packed ARGB integer.
    /// <para>
    /// Native platform APIs take colours as an int instead of a <see cref="Color"/>. The Android
    /// notification accent is one of them: a notification small icon is alpha-only, and the system
    /// paints it in the colour handed to <c>Notification.Builder.SetColor</c>.
    /// </para>
    /// </summary>
    public static int AccentArgb => Accent.ToInt();

    /// <summary>Current time indicator. Brighter than <see cref="Colors.Red"/> so it stays legible on the dark background.</summary>
    public static readonly Color CurrentTime = Color.FromArgb("#FF6B6B");
}
