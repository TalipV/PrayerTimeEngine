namespace PrayerTimeEngine.Presentation;

/// <summary>
/// Central type scale for the whole app.
/// <para>
/// The values are device-independent units (DIP). MAUI already divides by the display density,
/// so a value of 14 is physically about the same size on a small phone and on a flagship phone.
/// That is why there is no per-device lookup here: a bigger screen is meant to show more content,
/// not bigger text. Layouts have to adapt to the available space, font sizes do not.
/// </para>
/// <para>
/// The only axis that genuinely changes the reading distance is the device idiom, so tablets and
/// desktops get one step up.
/// </para>
/// </summary>
public static class AppFontSizes
{
    private static readonly bool s_isLargeIdiom =
        DeviceInfo.Idiom == DeviceIdiom.Tablet
        || DeviceInfo.Idiom == DeviceIdiom.Desktop
        || DeviceInfo.Idiom == DeviceIdiom.TV;

    private static double ForIdiom(double phone, double large) => s_isLargeIdiom ? large : phone;

    /// <summary>Profile name in the navigation bar.</summary>
    public static double Title => ForIdiom(21, 25);

    /// <summary>Secondary info in the navigation bar, e.g. the weeks-until text.</summary>
    public static double Subtitle => ForIdiom(17, 20);
}
