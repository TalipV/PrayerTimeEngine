using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic.VOs;

namespace PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic;

public class PrayerTimeGraphicView(
        ISystemInfoService systemInfoService
    ) : IDrawable
{
    private readonly Color _prayerTimeColor = AppColors.GraphicSurface;

    private readonly Color _prayerMainTextColor = AppColors.Text;
    private readonly Color _currentTimeTextColor = AppColors.CurrentTime;

    private readonly Color _prayerSubTimeBorderColor = AppColors.Border;
    private readonly Color _prayerSubTimeFillColor = AppColors.Card;
    private readonly Color _prayerSubTimeTextColor = AppColors.TextMuted;

    // gap between neighbouring sub time boxes, so they read as separate blocks instead of a table
    private const float SUB_TIME_INSET = 2;
    private const float SUB_TIME_CORNER_RADIUS = 3;

    public PrayerTimeGraphicTimeVO PrayerTimeGraphicTime { get; set; }

    public void Draw(ICanvas canvas, RectF fullRectangle)
    {
        if (PrayerTimeGraphicTime is null)
        {
            return;
        }

        canvas.FillColor = _prayerTimeColor;
        var mainGraphicRectangle =
            new RectF(
                x: 40,
                y: 40,
                width: fullRectangle.Width - 40,
                height: fullRectangle.Height - 40);
        canvas.FillRoundedRectangle(mainGraphicRectangle, 6.0);

        canvas.FontSize = 12;

        foreach (PrayerTimeGraphicSubTimeVO timeVO in PrayerTimeGraphicTime.SubTimeVOs)
        {
            DrawSubTime(
                canvas,
                mainGraphicRectangle,
                timeVO.Name,
                timeVO.Start,
                timeVO.End,
                type: timeVO.SubTimeType
            );
        }

        DrawPrayerTimeTexts(canvas, fullRectangle);
        DrawCurrentTimeIndicator(canvas, mainGraphicRectangle);
    }

    private void DrawCurrentTimeIndicator(ICanvas canvas, RectF baseRectangle)
    {
        ZonedDateTime currentZonedDateTime = systemInfoService.GetCurrentZonedDateTime();

        if (currentZonedDateTime.ToInstant() < PrayerTimeGraphicTime.Start.ToInstant() || currentZonedDateTime.ToInstant() > PrayerTimeGraphicTime.End.ToInstant())
        {
            // It's not within the time of DisplayPrayerTIme
            // so don't draw the indicator
            return;
        }

        float relativePos = GetRelativeDepthByInstant(currentZonedDateTime.ToInstant(), baseRectangle);

        var indicatorRectangle =
            new RectF(
                x: baseRectangle.X,
                y: baseRectangle.Y + relativePos,
                width: baseRectangle.Width,
                height: 2);

        canvas.FillColor = _currentTimeTextColor;
        canvas.FontColor = _currentTimeTextColor;
        canvas.FillRectangle(indicatorRectangle);

        // CURRENT TIME TEXT
        canvas.DrawString(
            currentZonedDateTime.ToString("HH:mm", null),
            x: indicatorRectangle.X - 40,
            y: indicatorRectangle.Y - 10,
            width: 40,
            height: 20,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private float GetRelativeDepthByInstant(Instant dateTime, RectF rectangle)
    {
        if (dateTime <= PrayerTimeGraphicTime.Start.ToInstant())
        {
            return 0;
        }

        float durationInSeconds = (float)(PrayerTimeGraphicTime.End.ToInstant() - PrayerTimeGraphicTime.Start.ToInstant()).TotalSeconds;
        float secondsSoFar = (float)(dateTime - PrayerTimeGraphicTime.Start.ToInstant()).TotalSeconds;

        float percentageOfDuration = secondsSoFar / durationInSeconds;

        return Math.Max(rectangle.Height * percentageOfDuration, 0);
    }

    private void DrawPrayerTimeTexts(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FontColor = _prayerMainTextColor;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontSize = 18f;

        // PRAYER NAME TEXT, centered above the body of the graphic
        canvas.DrawString(
            PrayerTimeGraphicTime.Title,
            x: 40,
            y: 4,
            width: dirtyRect.Width - 40,
            height: 30,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = _prayerMainTextColor;
        canvas.FontSize = 13f;

        // PRAYER TIME BEGINNING TEXT, level with the top edge of the body
        canvas.DrawString(
            PrayerTimeGraphicTime.Start.ToString("HH:mm", null),
            x: 0,
            y: 30,
            width: 36,
            height: 20,
            HorizontalAlignment.Right, VerticalAlignment.Center);

        // PRAYER TIME END TEXT, level with the bottom edge of the body
        canvas.DrawString(
            PrayerTimeGraphicTime.End.ToString("HH:mm", null),
            x: 0,
            y: dirtyRect.Height - 20,
            width: 36,
            height: 20,
            HorizontalAlignment.Right, VerticalAlignment.Center);
    }

    private void DrawSubTime(
        ICanvas canvas, RectF innerBackgroundRectangle, string name,
        Instant startDateTime, Instant endDateTime,
        ESubTimeType type)
    {
        float leftPos;
        float width;

        float regularWidth = innerBackgroundRectangle.Right - (innerBackgroundRectangle.Width / 2.0F);

        switch (type)
        {
            case ESubTimeType.FullHalf:
                leftPos = innerBackgroundRectangle.Width / 2.0f;
                width = innerBackgroundRectangle.Right - leftPos;
                break;
            case ESubTimeType.RightHalf:
                leftPos = (innerBackgroundRectangle.Width / 2.0f) + (regularWidth / 2.0f);
                width = innerBackgroundRectangle.Right - leftPos;
                break;
            case ESubTimeType.LeftHalf:
                leftPos = innerBackgroundRectangle.Width / 2.0f;
                width = innerBackgroundRectangle.Right - (regularWidth / 2.0f) - leftPos;
                break;
            default:
                throw new NotImplementedException($"{type} was not implemented!");
        }

        float topPos = innerBackgroundRectangle.Top + GetRelativeDepthByInstant(startDateTime, innerBackgroundRectangle);
        float height = GetRelativeDepthByInstant(endDateTime, innerBackgroundRectangle) - GetRelativeDepthByInstant(startDateTime, innerBackgroundRectangle);

        var innerSubtimeBackgroundRectangle =
            new RectF(
                x: leftPos + SUB_TIME_INSET,
                y: topPos + SUB_TIME_INSET,
                width: width - (2 * SUB_TIME_INSET),
                height: height - (2 * SUB_TIME_INSET)
            );

        if (innerSubtimeBackgroundRectangle.Height > 0)
        {
            canvas.FillColor = _prayerSubTimeFillColor;
            canvas.FillRoundedRectangle(innerSubtimeBackgroundRectangle, SUB_TIME_CORNER_RADIUS);

            canvas.StrokeColor = _prayerSubTimeBorderColor;
            canvas.StrokeSize = 1;
            canvas.DrawRoundedRectangle(innerSubtimeBackgroundRectangle, SUB_TIME_CORNER_RADIUS);

            if (innerSubtimeBackgroundRectangle.Height > 10)
            {
                canvas.FontColor = _prayerSubTimeTextColor;

                canvas.DrawString(
                    name,
                    innerSubtimeBackgroundRectangle.Center.X - 45,
                    innerSubtimeBackgroundRectangle.Center.Y - 10,
                    90,
                    20,
                    HorizontalAlignment.Center, VerticalAlignment.Center);
            }
        }
    }
}
