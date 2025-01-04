using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic.VOs;

namespace PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic;

public class PrayerTimeGraphicView(
        ISystemInfoService systemInfoService
    ) : IDrawable
{
    private readonly Color PrayerTimeColor = Colors.LightGray;

    private readonly Color PrayerMainTextColor = Colors.Black;
    private readonly Color CurrentTimeTextColor = Colors.Red;

    private readonly Color PrayerSubTimeBorderColor = Color.FromArgb("#f3eae3");
    private readonly Color PrayerSubTimeTextColor = Colors.Black;

    public PrayerTimeGraphicTimeVO PrayerTimeGraphicTime { get; set; }

    public void Draw(ICanvas canvas, RectF fullRectangle)
    {
        if (PrayerTimeGraphicTime is null)
            return;

        canvas.FillColor = PrayerTimeColor;
        var mainGraphicRectangle =
            new RectF(
                x: 40,
                y: 40,
                width: fullRectangle.Width - 40,
                height: fullRectangle.Height - 40);
        canvas.FillRoundedRectangle(mainGraphicRectangle, 15.0);

        canvas.FontSize = 12;

        foreach (PrayerTimeGraphicSubTimeVO timeVO in PrayerTimeGraphicTime.SubTimeVOs)
        {
            drawSubTime(
                canvas,
                mainGraphicRectangle,
                timeVO.Name,
                timeVO.Start,
                timeVO.End,
                type: timeVO.SubTimeType
            );
        }

        drawPrayerTimeTexts(canvas, fullRectangle);
        drawCurrentTimeIndicator(canvas, mainGraphicRectangle);
    }

    private void drawCurrentTimeIndicator(ICanvas canvas, RectF baseRectangle)
    {
        ZonedDateTime currentZonedDateTime = systemInfoService.GetCurrentZonedDateTime();

        if (currentZonedDateTime.ToInstant() < PrayerTimeGraphicTime.Start.ToInstant() || currentZonedDateTime.ToInstant() > PrayerTimeGraphicTime.End.ToInstant())
        {
            // It's not within the time of DisplayPrayerTIme
            // so don't draw the indicator
            return;
        }

        float relativePos = getRelativeDepthByInstant(currentZonedDateTime.ToInstant(), baseRectangle);

        var indicatorRectangle =
            new RectF(
                x: baseRectangle.X,
                y: baseRectangle.Y + relativePos,
                width: baseRectangle.Width,
                height: 2);

        canvas.FillColor = CurrentTimeTextColor;
        canvas.FontColor = CurrentTimeTextColor;
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

    private float getRelativeDepthByInstant(Instant dateTime, RectF rectangle)
    {
        float durationInSeconds = (float)(PrayerTimeGraphicTime.End.ToInstant() - PrayerTimeGraphicTime.Start.ToInstant()).TotalSeconds;
        float secondsSoFar = (float)(dateTime - PrayerTimeGraphicTime.Start.ToInstant()).TotalSeconds;

        float percentageOfDuration = secondsSoFar / durationInSeconds;

        return rectangle.Height * percentageOfDuration;
    }

    private void drawPrayerTimeTexts(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FontColor = PrayerMainTextColor;
        canvas.FontSize = 20f;

        // PRAYER NAME TEXT
        canvas.DrawString(
            PrayerTimeGraphicTime.Title,
            x: dirtyRect.Width / 2 - 40,
            y: 15,
            width: 90,
            height: 30,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.FontSize = 15f;

        // PRAYER TIME BEGINNING TEXT
        canvas.DrawString(
            PrayerTimeGraphicTime.Start.ToString("HH:mm", null),
            x: -25,
            y: 30,
            width: 90,
            height: 20,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        // PRAYER TIME END TEXT
        canvas.DrawString(
            PrayerTimeGraphicTime.End.ToString("HH:mm", null),
            x: -25,
            y: dirtyRect.Height - 20,
            width: 90,
            height: 20,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void drawSubTime(
        ICanvas canvas, RectF innerBackgroundRectangle, string name,
        Instant startDateTime, Instant endDateTime,
        ESubTimeType type)
    {
        float leftPos;
        float width;

        float regularWidth = innerBackgroundRectangle.Right - innerBackgroundRectangle.Width / 2.0F;

        switch (type)
        {
            case ESubTimeType.FullHalf:
                leftPos = innerBackgroundRectangle.Width / 2.0f;
                width = innerBackgroundRectangle.Right - leftPos;
                break;
            case ESubTimeType.RightHalf:
                leftPos = innerBackgroundRectangle.Width / 2.0f + regularWidth / 2.0f;
                width = innerBackgroundRectangle.Right - leftPos;
                break;
            case ESubTimeType.LeftHalf:
                leftPos = innerBackgroundRectangle.Width / 2.0f;
                width = innerBackgroundRectangle.Right - regularWidth / 2.0f - leftPos;
                break;
            default:
                throw new NotImplementedException($"{type} was not implemented!");
        }

        float topPos = innerBackgroundRectangle.Top + getRelativeDepthByInstant(startDateTime, innerBackgroundRectangle);
        float height = getRelativeDepthByInstant(endDateTime, innerBackgroundRectangle) - getRelativeDepthByInstant(startDateTime, innerBackgroundRectangle);

        var innerSubtimeBackgroundRectangle =
            new RectF(
                x: leftPos,
                y: topPos,
                width: width,
                height: height
            );

        if (innerSubtimeBackgroundRectangle.Height > 0)
        {
            canvas.FillColor = canvas.FontColor = canvas.StrokeColor = PrayerSubTimeBorderColor;
            canvas.DrawRectangle(innerSubtimeBackgroundRectangle);

            if (innerSubtimeBackgroundRectangle.Height > 10)
            {
                canvas.FontColor = PrayerSubTimeTextColor;

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
