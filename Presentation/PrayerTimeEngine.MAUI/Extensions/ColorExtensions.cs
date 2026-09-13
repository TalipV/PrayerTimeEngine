using Color = Mapsui.Styles.Color;

namespace PrayerTimeEngine.Extensions
{
    public static class ColorExtensions
    {
        public static Color WithTransparency(this Color color, int transparencyPercent)
        {
            if (transparencyPercent < 0 || transparencyPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(transparencyPercent));

            byte alpha = (byte)(255 * (transparencyPercent / 100d));

            return new Color(color.R, color.G, color.B, alpha);
        }
    }
}
