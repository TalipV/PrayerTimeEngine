using Android.Content.Res;
using Android.Graphics;
using Android.Util;
using PrayerTimeEngine.Presentation;
using Color = Android.Graphics.Color;
using Paint = Android.Graphics.Paint;

namespace PrayerTimeEngine.Platforms.Android.Notifications;

/// <summary>
/// Draws the progress bars of a prayer time together with the names of its sub times.
/// <para>
/// A custom notification layout can only use the handful of views which RemoteViews supports, and
/// none of them draws a bar split at arbitrary positions. Painting it ourselves keeps the segmented
/// look of <c>Notification.ProgressStyle</c> while staying visible in the collapsed notification -
/// which the style itself is not - and it is the only way to put each label right underneath the
/// transition it belongs to. A separate text view could only ever line them up on the left.
/// </para>
/// </summary>
internal static class PrayerTimeProgressBarRenderer
{
    private const float BAR_HEIGHT_DP = 6f;

    private const float LABEL_TEXT_SIZE_DP = 13f;
    private const float LABEL_GAP_DP = 5f;
    private const float SEGMENT_GAP_DP = 3f;

    /// <summary>Vertical space between the times underneath the bars and the durations below them.</summary>
    private const float SUB_ROW_GAP_DP = 2f;

    /// <summary>Breathing room between a sub time label and the start or end time next to it.</summary>
    private const float LABEL_PADDING_DP = 6f;

    private const int TRACK_ALPHA = 0x40;

    /// <summary>Deliberately the bar height: a wait is drawn differently, but not weaker.</summary>
    private const float CONNECTOR_STROKE_DP = BAR_HEIGHT_DP;

    /// <summary>Marks the current moment on the line. The only dot left, the ends carry none.</summary>
    private const float CONNECTOR_MARKER_RADIUS_DP = 4f;

    // with a round cap the dashes come out as dots, so "on" stays short and the gap carries the rhythm
    private const float CONNECTOR_DASH_ON_DP = 2f;
    private const float CONNECTOR_DASH_OFF_DP = 8f;

    /// <summary>
    /// Text colour of the labels, chosen for the notification background of the current theme.
    /// </summary>
    private static Color getLabelColor()
    {
        Configuration configuration = getAndroidContextResources().Configuration
            ?? throw new Exception("Failed to retrieve Android resource configuration");

        UiMode uiMode = configuration.UiMode
            & UiMode.NightMask;

        return uiMode == UiMode.NightYes
            ? Color.Argb(0xCC, 0xFF, 0xFF, 0xFF)
            : Color.Argb(0xCC, 0x00, 0x00, 0x00);
    }

    /// <param name="segmentLengths">Length of each section in percent, adding up to 100.</param>
    /// <param name="labels">Name of each transition between two sections, so one less than there are sections.</param>
    /// <param name="startText">Start of the prayer time, at the left end underneath the bars.</param>
    /// <param name="endText">End of the prayer time, at the right end underneath the bars.</param>
    /// <param name="startSubText">Optional second line below <paramref name="startText"/>.</param>
    /// <param name="endSubText">Optional second line below <paramref name="endText"/>.</param>
    /// <param name="drawAsConnector">
    /// Draws a dotted line between two end points instead of a solid bar. Used for the span between
    /// two prayer times, which is a wait rather than a prayer time, and should not look like one.
    /// </param>
    public static Bitmap Render(
        int elapsedPercent,
        int[] segmentLengths,
        string[] labels,
        string startText,
        string endText,
        string? startSubText = null,
        string? endSubText = null,
        bool drawAsConnector = false)
    {
        DisplayMetrics displayMetrics = getAndroidContextResources().DisplayMetrics
            ?? throw new Exception("Failed to retrieve Android resource display metrics");

        // drawn at roughly the width it will be shown at, so the labels are not scaled sideways
        int bitmapWidth = Math.Clamp(displayMetrics.WidthPixels, 480, 2000);
        float density = displayMetrics.Density;

        float barHeight = BAR_HEIGHT_DP * density;
        float segmentGap = SEGMENT_GAP_DP * density;

        Paint paint = new Paint { AntiAlias = true, TextSize = LABEL_TEXT_SIZE_DP * density };

        // a line of text reaches above and below its baseline, and the part below is what holds the
        // descenders of a "y" or a "g". Sizing rows by the font size alone would cut those off.
        Paint.FontMetrics fontMetrics = paint.GetFontMetrics()
            ?? throw new Exception("Failed to retrieve font metrics");
        float textAscent = -fontMetrics.Ascent;
        float textDescent = fontMetrics.Descent;
        float textHeight = textAscent + textDescent;

        float barAreaHeight = drawAsConnector
            ? Math.Max(CONNECTOR_STROKE_DP, 2f * CONNECTOR_MARKER_RADIUS_DP) * density
            : barHeight;
        float rowHeight = barAreaHeight + LABEL_GAP_DP * density + textHeight;

        bool hasSubTexts = !string.IsNullOrEmpty(startSubText) || !string.IsNullOrEmpty(endSubText);
        float subRowHeight = hasSubTexts ? SUB_ROW_GAP_DP * density + textHeight : 0f;

        float totalHeight = rowHeight + subRowHeight;

        Bitmap bitmap = Bitmap.CreateBitmap(bitmapWidth, (int)Math.Ceiling(totalHeight), Bitmap.Config.Argb8888);

        Canvas canvas = new Canvas(bitmap);

        Color accentColor = new Color(AppColors.AccentArgb);
        Color trackColor = Color.Argb(TRACK_ALPHA, accentColor.R, accentColor.G, accentColor.B);

        float baselineY = rowHeight - textDescent;

        List<float> boundaryPositions = drawAsConnector
            ? drawConnector(canvas, paint, elapsedPercent, barAreaHeight, bitmapWidth, density, accentColor, trackColor)
            : drawBar(canvas, paint, segmentLengths, elapsedPercent, barHeight, bitmapWidth, segmentGap, accentColor, trackColor);

        drawTexts(canvas, paint, labels, boundaryPositions, bitmapWidth, baselineY, density, startText, endText);

        if (hasSubTexts)
            drawSubTexts(canvas, paint, bitmapWidth, baselineY + subRowHeight, startSubText, endSubText);

        return bitmap;
    }

    /// <summary>
    /// A dotted line with a small dot marking the current moment on it. Deliberately nothing like the
    /// solid bar of a running prayer time.
    /// </summary>
    /// <returns>No transitions to label, a wait is not divided into anything.</returns>
    private static List<float> drawConnector(
        Canvas canvas,
        Paint paint,
        int elapsedPercent,
        float areaHeight,
        int bitmapWidth,
        float density,
        Color accentColor,
        Color trackColor)
    {
        float strokeWidth = CONNECTOR_STROKE_DP * density;
        float centerY = areaHeight / 2f;

        // the round caps would stick out over the edges of the bitmap otherwise
        float lineStart = strokeWidth / 2f;
        float lineEnd = bitmapWidth - strokeWidth / 2f;
        float progressX = lineStart + (lineEnd - lineStart) * elapsedPercent / 100f;

        paint.SetStyle(Paint.Style.Stroke);
        paint.StrokeWidth = strokeWidth;
        paint.StrokeCap = Paint.Cap.Round;
        paint.SetPathEffect(new DashPathEffect([CONNECTOR_DASH_ON_DP * density, CONNECTOR_DASH_OFF_DP * density], 0f));

        paint.Color = trackColor;
        canvas.DrawLine(lineStart, centerY, lineEnd, centerY, paint);

        paint.Color = accentColor;

        if (progressX > lineStart)
            canvas.DrawLine(lineStart, centerY, progressX, centerY, paint);

        paint.SetPathEffect(null);
        paint.SetStyle(Paint.Style.Fill);

        canvas.DrawCircle(progressX, centerY, CONNECTOR_MARKER_RADIUS_DP * density, paint);

        return [];
    }

    /// <summary>
    /// Second line underneath the two times, for whatever belongs directly to them.
    /// </summary>
    private static void drawSubTexts(
        Canvas canvas,
        Paint paint,
        int bitmapWidth,
        float baselineY,
        string startSubText,
        string endSubText)
    {
        if (!string.IsNullOrEmpty(startSubText))
        {
            paint.TextAlign = Paint.Align.Left;
            canvas.DrawText(startSubText, 0f, baselineY, paint);
        }

        if (!string.IsNullOrEmpty(endSubText))
        {
            paint.TextAlign = Paint.Align.Right;
            canvas.DrawText(endSubText, bitmapWidth, baselineY, paint);
        }
    }

    /// <returns>
    /// The x position of every transition between two sections, for the labels below.
    /// </returns>
    private static List<float> drawBar(
        Canvas canvas,
        Paint paint,
        int[] segmentLengths,
        int elapsedPercent,
        float barHeight,
        int bitmapWidth,
        float segmentGap,
        Color accentColor,
        Color trackColor)
    {
        float radius = barHeight / 2f;
        float usableWidth = bitmapWidth - segmentGap * Math.Max(0, segmentLengths.Length - 1);

        List<float> boundaryPositions = new List<float>();
        float left = 0f;
        int consumedPercent = 0;

        foreach (int segmentLength in segmentLengths)
        {
            float segmentWidth = usableWidth * segmentLength / 100f;

            paint.Color = trackColor;
            canvas.DrawRoundRect(left, 0f, left + segmentWidth, barHeight, radius, radius, paint);

            // how much of this particular section lies behind the current moment
            float filledRatio = segmentLength <= 0
                ? 0f
                : Math.Clamp((elapsedPercent - consumedPercent) / (float)segmentLength, 0f, 1f);

            if (filledRatio > 0f)
            {
                paint.Color = accentColor;
                canvas.DrawRoundRect(left, 0f, left + segmentWidth * filledRatio, barHeight, radius, radius, paint);
            }

            consumedPercent += segmentLength;
            left += segmentWidth;

            boundaryPositions.Add(left + segmentGap / 2f);
            left += segmentGap;
        }

        return boundaryPositions;
    }

    private static void drawTexts(
        Canvas canvas,
        Paint paint,
        string[] labels,
        List<float> boundaryPositions,
        int bitmapWidth,
        float baselineY,
        float density,
        string startText,
        string endText)
    {
        paint.Color = getLabelColor();
        paint.TextAlign = Paint.Align.Center;

        // bars above the bottom one leave the times out, the room goes to their own labels instead
        float startTextWidth = string.IsNullOrEmpty(startText) ? 0f : paint.MeasureText(startText);
        float endTextWidth = string.IsNullOrEmpty(endText) ? 0f : paint.MeasureText(endText);

        if (startTextWidth > 0f)
        {
            paint.TextAlign = Paint.Align.Left;
            canvas.DrawText(startText, 0f, baselineY, paint);
        }

        if (endTextWidth > 0f)
        {
            paint.TextAlign = Paint.Align.Right;
            canvas.DrawText(endText, bitmapWidth, baselineY, paint);
        }

        paint.TextAlign = Paint.Align.Center;

        // the sub time labels have to stay clear of those two times
        float padding = LABEL_PADDING_DP * density;
        float lowerBound = startTextWidth + padding;
        float upperBound = bitmapWidth - endTextWidth - padding;

        for (int i = 0; i < labels.Length && i < boundaryPositions.Count; i++)
        {
            string label = labels[i];
            float halfTextWidth = paint.MeasureText(label) / 2f;

            float minX = lowerBound + halfTextWidth;
            float maxX = upperBound - halfTextWidth;

            // no room left between the start and end time, so the label would only overlap them
            if (minX > maxX)
                continue;

            canvas.DrawText(label, Math.Clamp(boundaryPositions[i], minX, maxX), baselineY, paint);
        }
    }

    private static Resources getAndroidContextResources()
    {
        return global::Android.App.Application.Context.Resources
            ?? throw new Exception("Failed to retrieve Android context resources");
    }
}
