using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using PrayerTimeEngine.Domain.Model;
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

            DrawBaseInformation(canvas, fullRectangle);
        }

        private void DrawBaseInformation(ICanvas canvas, RectF fullRectangle)
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

            drawPrayerTimeTexts(canvas, fullRectangle);
            drawCurrentTimeIndicator(canvas, mainGraphicRectangle);
        }

        private void drawCurrentTimeIndicator(ICanvas canvas, RectF baseRectangle)
        {
            DateTime dateTime = DateTime.Now;

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
            if (dateTime < DisplayPrayerTime.Start || dateTime > DisplayPrayerTime.End)
            {
                return 0;
            }

            float secondDuration = (float) (DisplayPrayerTime.End.Value - DisplayPrayerTime.Start.Value).TotalSeconds;
            float secondsSoFar = (float) (dateTime - DisplayPrayerTime.Start.Value).TotalSeconds;

            float percentageOfDuration = secondsSoFar / secondDuration;

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
    }
}
