using PrayerTimeEngine.Domain.Model;
using System.Xml.Linq;
using System;

namespace PrayerTimeEngine.Presentation.GraphicsView
{
    public class PrayerTimeGraphicView : IDrawable
    {
        private Color MainBackgroundColor = Color.FromRgba(35, 41, 53, 255);
        private Color PrayerTimeColor = Color.FromRgb(124, 197, 107);

        private Color PrayerMainTextColor = Colors.Yellow;
        private Color CurrentTimeTextColor = Colors.Red;

        private Color PrayerSubTimeBackgroundColor = Color.FromRgb(90, 187, 71);
        private Color PrayerSubTimeBorderColor = Colors.White;
        private Color PrayerSubTimeTextColor = Colors.Cyan;

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
            canvas.FillColor = MainBackgroundColor;
            canvas.FillRectangle(fullRectangle);

            RectF mainGraphicRectangle =
                new RectF(
                    x: 40,
                    y: 40,
                    width: fullRectangle.Width - 40,
                    height: fullRectangle.Height - 40);
            canvas.FillColor = PrayerTimeColor;
            canvas.FillRoundedRectangle(mainGraphicRectangle, 15.0);

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
            DateTime dateTime = DateTime.Now;

            if (dateTime < DisplayPrayerTime.Start || dateTime > DisplayPrayerTime.End)
            {
                // It's not within the time of DisplayPrayerTIme
                // so don't draw the indicator
                return;
            }

            float relativePos = getRelativeDepthByTime(dateTime, baseRectangle);

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
                dateTime.ToString("HH:mm"),
                x: indicatorRectangle.X - 40,
                y: indicatorRectangle.Y - 10,
                width: 40,
                height: 20,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        private float getRelativeDepthByTime(DateTime dateTime, RectF rectangle)
        {
            float durationInSeconds = (float) (DisplayPrayerTime.End.Value - DisplayPrayerTime.Start.Value).TotalSeconds;
            float secondsSoFar = (float) (dateTime - DisplayPrayerTime.Start.Value).TotalSeconds;

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
                x: (dirtyRect.Width/2) - 40, 
                y: 15,
                width: 90,
                height: 20,
                HorizontalAlignment.Center, VerticalAlignment.Center);

            canvas.FontSize = 15f;
            
            // PRAYER TIME BEGINNING TEXT
            canvas.DrawString(
                DisplayPrayerTime.Start.Value.ToString("HH:mm"),
                x: -25,
                y: 30,
                width: 90,
                height: 20,
                HorizontalAlignment.Center, VerticalAlignment.Center);

            // PRAYER TIME END TEXT
            canvas.DrawString(
                DisplayPrayerTime.End.Value.ToString("HH:mm"),
                x: -25,
                y: dirtyRect.Height - 20,
                width: 90,
                height: 20,
                HorizontalAlignment.Center, VerticalAlignment.Center);
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
                asrPrayerTime.Start.Value,
                asrPrayerTime.Mithlayn.Value
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Normal",
                asrPrayerTime.Mithlayn.Value,
                asrPrayerTime.Karaha.Value
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Karaha",
                asrPrayerTime.Karaha.Value,
                asrPrayerTime.End.Value
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
                maghribPrayerTime.Start.Value,
                maghribPrayerTime.SufficientTime.Value
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Karaha1",
                maghribPrayerTime.SufficientTime.Value,
                maghribPrayerTime.Ishtibaq.Value
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "Karaha2",
                maghribPrayerTime.Ishtibaq.Value,
                maghribPrayerTime.End.Value
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
                ishaPrayerTime.Start.Value,
                ishaPrayerTime.FirstThirdOfNight.Value,
                1
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "2/3",
                ishaPrayerTime.FirstThirdOfNight.Value,
                ishaPrayerTime.SecondThirdOfNight.Value,
                1
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "3/3",
                ishaPrayerTime.SecondThirdOfNight.Value,
                ishaPrayerTime.End.Value,
                1
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "1/2",
                ishaPrayerTime.Start.Value,
                ishaPrayerTime.MiddleOfNight.Value,
                2
            );

            drawSubTime(
                canvas: canvas,
                mainGraphicRectangle,
                name: "2/2",
                ishaPrayerTime.MiddleOfNight.Value,
                ishaPrayerTime.End.Value,
                2
            );
        }

        private void drawSubTime(
            ICanvas canvas, RectF innerBackgroundRectangle, string name, 
            DateTime startDateTime, DateTime endDateTime, 
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

            float topPos = innerBackgroundRectangle.Top + getRelativeDepthByTime(startDateTime, innerBackgroundRectangle);
            float height = getRelativeDepthByTime(endDateTime, innerBackgroundRectangle) - getRelativeDepthByTime(startDateTime, innerBackgroundRectangle);

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
                        innerSubtimeBackgroundRectangle.Center.X,
                        innerSubtimeBackgroundRectangle.Center.Y,
                        HorizontalAlignment.Center);
                }
            }
        }
    }
}
