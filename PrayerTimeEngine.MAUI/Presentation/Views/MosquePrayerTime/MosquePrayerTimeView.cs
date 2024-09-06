using OnScreenSizeMarkup.Maui.Helpers;
using PrayerTimeEngine.Core.Domain.Models;

namespace PrayerTimeEngine.Presentation.Views.MosquePrayerTime
{
    public class MosquePrayerTimeView : ContentView
    {
        public MosquePrayerTimeView()
        {
            Content = createUI();
        }

        private static Grid createUI()
        {
            var mainGrid = new Grid
            {
                Padding = new Thickness(10, 20, 10, 20),
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) }
                }
            };

            int startRowNo = 1;

            string bindingBeginning = $"{nameof(MosquePrayerTimeViewModel.PrayerTimesCollection)}";

            string bindingText = $"{nameof(MosquePrayerTimeViewModel.PrayerTimesCollection)}.{nameof(PrayerTimesCollection.Fajr)}.{nameof(AbstractPrayerTime.DurationDisplayText)}";
            addPrayerTimeUI(mainGrid, "Fajr", durationBinding: bindingText,
                startRowNo: startRowNo, startColumnNo: 0);

            //bindingText = $"{nameof(MosquePrayerTimeViewModel.PrayerTimesCollection)}.{nameof(PrayerTimesCollection.Jumuah)}.{nameof(AbstractPrayerTime.DurationDisplayText)}";
            //addPrayerTimeUI(mainGrid, "Duha", durationBinding: $"{bindingText}.{nameof(AbstractPrayerTime.DurationDisplayText)}",
            //    startRowNo: startRowNo, startColumnNo: 3);

            bindingText = $"{nameof(MosquePrayerTimeViewModel.PrayerTimesCollection)}.{nameof(PrayerTimesCollection.Dhuhr)}.{nameof(AbstractPrayerTime.DurationDisplayText)}";
            addPrayerTimeUI(mainGrid, "Dhuhr", durationBinding: bindingText,
                startRowNo: startRowNo + 4, startColumnNo: 0);

            bindingText = $"{nameof(MosquePrayerTimeViewModel.PrayerTimesCollection)}.{nameof(PrayerTimesCollection.Asr)}.{nameof(AbstractPrayerTime.DurationDisplayText)}";
            addPrayerTimeUI(mainGrid, "Asr", durationBinding: bindingText,
                startRowNo: startRowNo + 4, startColumnNo: 3);

            bindingText = $"{nameof(MosquePrayerTimeViewModel.PrayerTimesCollection)}.{nameof(PrayerTimesCollection.Maghrib)}.{nameof(AbstractPrayerTime.DurationDisplayText)}";
            addPrayerTimeUI(mainGrid, "Maghrib", durationBinding: bindingText,
                startRowNo: startRowNo + 8, startColumnNo: 0);

            bindingText = $"{nameof(MosquePrayerTimeViewModel.PrayerTimesCollection)}.{nameof(PrayerTimesCollection.Isha)}.{nameof(AbstractPrayerTime.DurationDisplayText)}";
            addPrayerTimeUI(mainGrid, "Isha", durationBinding: bindingText,
                startRowNo: startRowNo + 8, startColumnNo: 3);

            return mainGrid;
        }

        private static void addPrayerTimeUI(
            Grid grid,
            string prayerName,
            string durationBinding,
            int startRowNo, int startColumnNo)
        {
            List<Label> timeTextViews = [];
            List<Label> timeDisplayTextViews = [];
            List<Label> subTimeTextViews = [];
            List<Label> subTimeDisplayTextViews = [];

            var prayerNameLabel = new Label
            {
                Text = prayerName,
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            };
            grid.AddWithSpan(prayerNameLabel, startRowNo, startColumnNo, columnSpan: 2);

            var prayerDurationLabel = new Label
            {
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            };
            prayerDurationLabel.SetBinding(Label.TextProperty, new Binding(durationBinding, stringFormat: "{0:HH:mm:ss}"));
            grid.AddWithSpan(prayerDurationLabel, startRowNo + 1, startColumnNo, columnSpan: 2);

            timeTextViews.Add(prayerNameLabel);
            timeDisplayTextViews.Add(prayerDurationLabel);

            if (OperatingSystem.IsWindows())
            {
                return;
            }

            // PRAYER TIME MAIN TITLES
            timeTextViews.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.Instance.GetScreenSizeValue<double>(
                        defaultSize: DebugUtil.GetSizeValues(99)[1],
                        extraLarge: DebugUtil.GetSizeValues(99)[1],
                        large: DebugUtil.GetSizeValues(24)[1],
                        medium: DebugUtil.GetSizeValues(22)[1],
                        small: DebugUtil.GetSizeValues(99)[1],
                        extraSmall: DebugUtil.GetSizeValues(99)[1]);
            });

            // PRAYER TIME MAIN DURATIONS
            timeDisplayTextViews.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.Instance.GetScreenSizeValue<double>(
                        defaultSize: DebugUtil.GetSizeValues(99)[2],
                        extraLarge: DebugUtil.GetSizeValues(99)[2],
                        large: DebugUtil.GetSizeValues(18)[2],
                        medium: DebugUtil.GetSizeValues(14)[2],
                        small: DebugUtil.GetSizeValues(99)[2],
                        extraSmall: DebugUtil.GetSizeValues(99)[2]);
            });

            subTimeTextViews.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.Instance.GetScreenSizeValue<double>(
                        defaultSize: DebugUtil.GetSizeValues(99)[3],
                        extraLarge: DebugUtil.GetSizeValues(99)[3],
                        large: DebugUtil.GetSizeValues(14)[3],
                        medium: DebugUtil.GetSizeValues(12)[3],
                        small: DebugUtil.GetSizeValues(99)[3],
                        extraSmall: DebugUtil.GetSizeValues(99)[3]);
            });

            subTimeDisplayTextViews.ForEach(label =>
            {
                label.FontSize =
                    OnScreenSizeHelpers.Instance.GetScreenSizeValue<double>(
                        defaultSize: DebugUtil.GetSizeValues(99)[4],
                        extraLarge: DebugUtil.GetSizeValues(99)[4],
                        large: DebugUtil.GetSizeValues(14)[4],
                        medium: DebugUtil.GetSizeValues(11)[4],
                        small: DebugUtil.GetSizeValues(99)[4],
                        extraSmall: DebugUtil.GetSizeValues(99)[4]);
            });
        }
    }
}
