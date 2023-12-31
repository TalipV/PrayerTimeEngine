﻿using NodaTime;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Presentation.GraphicsView
{
    public class PrayerTimeGraphicView : IDrawable
    {
        private Color MainBackgroundColor = Color.FromRgba(35, 41, 53, 255);
        private Color PrayerTimeColor = Colors.LightGray;

        private Color PrayerMainTextColor = Colors.Black;
        private Color CurrentTimeTextColor = Colors.Red;

        private Color PrayerSubTimeBackgroundColor = Color.FromRgb(90, 187, 71);
        private Color PrayerSubTimeBorderColor = Colors.BlueViolet;
        private Color PrayerSubTimeTextColor = Colors.Black;

        private DateTimeZone _timeZone { get; } = DateTimeZoneProviders.Tzdb[TimeZoneInfo.Local.Id];

        public PrayerTime DisplayPrayerTime { get; set; }

        public void Draw(ICanvas canvas, RectF fullRectangle)
        {
            if (DisplayPrayerTime == null
                || DisplayPrayerTime.Start == null
                || DisplayPrayerTime.End == null)
            {
                return;
            }

            drawInternal(canvas, fullRectangle);
        }

        private void drawInternal(ICanvas canvas, RectF fullRectangle)
        {
            RectF mainGraphicRectangle =
                new RectF(
                    x: 40,
                    y: 40,
                    width: fullRectangle.Width - 40,
                    height: fullRectangle.Height - 40);
            canvas.FillColor = PrayerTimeColor;
            canvas.FillRoundedRectangle(mainGraphicRectangle, 15.0);

            canvas.FontSize = 12;

            if (DisplayPrayerTime is FajrPrayerTime fajrPrayerTime)
            {
                drawFajrSubtimes(canvas, mainGraphicRectangle, fajrPrayerTime);
            }            
            if (DisplayPrayerTime is DuhaPrayerTime duhaPrayer)
            {
                drawDuhaSubtimes(canvas, mainGraphicRectangle, duhaPrayer);
            }            
            if (DisplayPrayerTime is AsrPrayerTime asrPrayerTime)
            {
                drawAsrSubtimes(canvas, mainGraphicRectangle, asrPrayerTime);
            }
            else if (DisplayPrayerTime is MaghribPrayerTime maghribPrayerTime)
            {
                drawMaghribSubtimes(canvas, mainGraphicRectangle, maghribPrayerTime);
            }
            else if (DisplayPrayerTime is IshaPrayerTime ishaPrayerTime)
            {
                drawIshaSubtimes(canvas, mainGraphicRectangle, ishaPrayerTime);
            }

            drawPrayerTimeTexts(canvas, fullRectangle);
            drawCurrentTimeIndicator(canvas, mainGraphicRectangle);
        }

        private void drawCurrentTimeIndicator(ICanvas canvas, RectF baseRectangle)
        {
            Instant currentInstant = SystemClock.Instance.GetCurrentInstant();

            if (currentInstant < DisplayPrayerTime.Start.Value.ToInstant() || currentInstant > DisplayPrayerTime.End.Value.ToInstant())
            {
                // It's not within the time of DisplayPrayerTIme
                // so don't draw the indicator
                return;
            }

            float relativePos = getRelativeDepthByInstant(currentInstant, baseRectangle);

            RectF indicatorRectangle =
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
                currentInstant.InZone(_timeZone).ToString("HH:mm", null),
                x: indicatorRectangle.X - 40,
                y: indicatorRectangle.Y - 10,
                width: 40,
                height: 20,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        private float getRelativeDepthByInstant(Instant dateTime, RectF rectangle)
        {
            float durationInSeconds = (float)(DisplayPrayerTime.End.Value.ToInstant() - DisplayPrayerTime.Start.Value.ToInstant()).TotalSeconds;
            float secondsSoFar = (float) (dateTime - DisplayPrayerTime.Start.Value.ToInstant()).TotalSeconds;

            float percentageOfDuration = secondsSoFar / durationInSeconds;

            return rectangle.Height * percentageOfDuration;
        }

        private void drawPrayerTimeTexts(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FontColor = PrayerMainTextColor;
            canvas.FontSize = 20f;

            // PRAYER NAME TEXT
            canvas.DrawString(
                DisplayPrayerTime.Name,
                x: (dirtyRect.Width / 2) - 40, 
                y: 15,
                width: 90,
                height: 30,
                HorizontalAlignment.Center, VerticalAlignment.Center);

            canvas.FontSize = 15f;
            
            // PRAYER TIME BEGINNING TEXT
            canvas.DrawString(
                DisplayPrayerTime.Start.Value.WithZone(_timeZone).ToString("HH:mm", null),
                x: -25,
                y: 30,
                width: 90,
                height: 20,
                HorizontalAlignment.Center, VerticalAlignment.Center);

            // PRAYER TIME END TEXT
            canvas.DrawString(
                DisplayPrayerTime.End.Value.WithZone(_timeZone).ToString("HH:mm", null),
                x: -25,
                y: dirtyRect.Height - 20,
                width: 90,
                height: 20,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        private void drawFajrSubtimes(ICanvas canvas, RectF mainGraphicRectangle, FajrPrayerTime fajrPrayerTime)
        {
            if (fajrPrayerTime.Ghalas == null || fajrPrayerTime.Karaha == null)
            {
                return;
            }

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Ikhtiyar",
                fajrPrayerTime.Start.Value.ToInstant(),
                fajrPrayerTime.Ghalas.Value.ToInstant()
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Normal",
                fajrPrayerTime.Ghalas.Value.ToInstant(),
                fajrPrayerTime.Karaha.Value.ToInstant()
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Karaha",
                fajrPrayerTime.Karaha.Value.ToInstant(),
                fajrPrayerTime.End.Value.ToInstant()
            );
        }

        private void drawDuhaSubtimes(ICanvas canvas, RectF mainGraphicRectangle, DuhaPrayerTime duhaPrayerTime)
        {
            if (duhaPrayerTime.QuarterOfDay == null)
            {
                return;
            }

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Normal",
                duhaPrayerTime.Start.Value.ToInstant(),
                duhaPrayerTime.QuarterOfDay.Value.ToInstant()
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Empfohlen",
                duhaPrayerTime.QuarterOfDay.Value.ToInstant(),
                duhaPrayerTime.End.Value.ToInstant()
            );
        }

        private void drawAsrSubtimes(ICanvas canvas, RectF mainGraphicRectangle, AsrPrayerTime asrPrayerTime)
        {
            if (asrPrayerTime.Mithlayn == null || asrPrayerTime.Karaha == null)
            {
                return;
            }

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Ikhtiyar",
                asrPrayerTime.Start.Value.ToInstant(),
                asrPrayerTime.Mithlayn.Value.ToInstant()
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Normal",
                asrPrayerTime.Mithlayn.Value.ToInstant(),
                asrPrayerTime.Karaha.Value.ToInstant()
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Karaha",
                asrPrayerTime.Karaha.Value.ToInstant(),
                asrPrayerTime.End.Value.ToInstant()
            );
        }

        private void drawMaghribSubtimes(ICanvas canvas, RectF mainGraphicRectangle, MaghribPrayerTime maghribPrayerTime)
        {
            if (maghribPrayerTime.SufficientTime == null || maghribPrayerTime.Ishtibaq == null)
            {
                return;
            }

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Normal",
                maghribPrayerTime.Start.Value.ToInstant(),
                maghribPrayerTime.SufficientTime.Value.ToInstant()
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Karaha1",
                maghribPrayerTime.SufficientTime.Value.ToInstant(),
                maghribPrayerTime.Ishtibaq.Value.ToInstant()
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Karaha2",
                maghribPrayerTime.Ishtibaq.Value.ToInstant(),
                maghribPrayerTime.End.Value.ToInstant()
            );
        }

        private void drawIshaSubtimes(ICanvas canvas, RectF mainGraphicRectangle, IshaPrayerTime ishaPrayerTime)
        {
            if (ishaPrayerTime.FirstThirdOfNight == null 
                || ishaPrayerTime.SecondThirdOfNight == null
                || ishaPrayerTime.MiddleOfNight == null)
            {
                return;
            }

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "1/3",
                ishaPrayerTime.Start.Value.ToInstant(),
                ishaPrayerTime.FirstThirdOfNight.Value.ToInstant(),
                1
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "2/3",
                ishaPrayerTime.FirstThirdOfNight.Value.ToInstant(),
                ishaPrayerTime.SecondThirdOfNight.Value.ToInstant(),
                1
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "3/3",
                ishaPrayerTime.SecondThirdOfNight.Value.ToInstant(),
                ishaPrayerTime.End.Value.ToInstant(),
                1
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "1/2",
                ishaPrayerTime.Start.Value.ToInstant(),
                ishaPrayerTime.MiddleOfNight.Value.ToInstant(),
                2
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "2/2",
                ishaPrayerTime.MiddleOfNight.Value.ToInstant(),
                ishaPrayerTime.End.Value.ToInstant(),
                2
            );
        }

        private void drawSubTime(
            ICanvas canvas, RectF innerBackgroundRectangle, string name, 
            Instant startDateTime, Instant endDateTime, 
            int displayMode = 0)
        {
            float leftPos = 0.0f;
            float width = 0.0f;

            float regularWidth = innerBackgroundRectangle.Right - innerBackgroundRectangle.Width / 2.0F;

            switch(displayMode) 
            {
                // normal auf volle Länge
                case 0:
                    leftPos = innerBackgroundRectangle.Width / 2.0f;
                    width = innerBackgroundRectangle.Right - leftPos;
                    break;
                // rechte Hälfte
                case 1:
                    leftPos = (innerBackgroundRectangle.Width / 2.0f) + (regularWidth / 2.0f);
                    width = innerBackgroundRectangle.Right - leftPos;
                    break;
                // linke Hälfte
                case 2:
                    leftPos = innerBackgroundRectangle.Width / 2.0f;
                    width = innerBackgroundRectangle.Right - (regularWidth / 2.0f) - leftPos;
                    break;
            }

            float topPos = innerBackgroundRectangle.Top + getRelativeDepthByInstant(startDateTime, innerBackgroundRectangle);
            float height = getRelativeDepthByInstant(endDateTime, innerBackgroundRectangle) - getRelativeDepthByInstant(startDateTime, innerBackgroundRectangle);

            RectF innerSubtimeBackgroundRectangle =
                new RectF(
                    x: leftPos,
                    y: topPos,
                    width: width,
                    height: height
                );

            if (innerSubtimeBackgroundRectangle.Height > 0)
            {
                canvas.FillColor = PrayerSubTimeBackgroundColor;
                canvas.DrawRoundedRectangle(innerSubtimeBackgroundRectangle, 0f);
                canvas.FillColor = PrayerSubTimeBorderColor;
                canvas.DrawRectangle(innerSubtimeBackgroundRectangle);

                if (innerSubtimeBackgroundRectangle.Height > 10)
                {
                    canvas.FontColor = PrayerSubTimeTextColor;

                    canvas.DrawString(
                        name,
                        innerSubtimeBackgroundRectangle.Center.X - 25,
                        innerSubtimeBackgroundRectangle.Center.Y - 10,
                        50,
                        20,
                        HorizontalAlignment.Center, VerticalAlignment.Center);
                }
            }
        }
    }
}
