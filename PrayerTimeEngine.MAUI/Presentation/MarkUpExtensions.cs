namespace PrayerTimeEngine.Presentation
{
    /// <summary>
    /// Just for me to add mark up extension methods which are missing
    /// </summary>
    internal static class MarkUpExtensions
    {
        public static Label LineBreakMode(this Label label, LineBreakMode lineBreakMode)
        {
            label.LineBreakMode = lineBreakMode;
            return label;
        }
    }
}
