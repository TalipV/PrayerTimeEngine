using OnScreenSizeMarkup.Maui.Helpers;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

namespace PrayerTimeEngine.Presentation.Views.MosquePrayerTimes;

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

        string bindingText = $"{nameof(MosquePrayerTimeViewModel.PrayerTimesSet)}.{nameof(MosquePrayerTimesSet.Fajr)}.{nameof(MosquePrayerTime.Start)}";
        addPrayerTimeUI(mainGrid, "Fajr", durationBinding: bindingText,
            startRowNo: startRowNo, startColumnNo: 0);

        bindingText = $"{nameof(MosquePrayerTimeViewModel.PrayerTimesSet)}.{nameof(MosquePrayerTimesSet.Jumuah)}.{nameof(MosquePrayerTime.Start)}";
        addPrayerTimeUI(mainGrid, "Jumu'ah", durationBinding: bindingText,
            startRowNo: startRowNo, startColumnNo: 3,
            subtime1Name: "Jumuah2", subtime1Binding: $"{nameof(MosquePrayerTimeViewModel.PrayerTimesSet)}.{nameof(MosquePrayerTimesSet.Jumuah2)}.{nameof(MosquePrayerTime.Start)}");

        bindingText = $"{nameof(MosquePrayerTimeViewModel.PrayerTimesSet)}.{nameof(MosquePrayerTimesSet.Dhuhr)}.{nameof(MosquePrayerTime.Start)}";
        addPrayerTimeUI(mainGrid, "Dhuhr", durationBinding: bindingText,
            startRowNo: startRowNo + 4, startColumnNo: 0);

        bindingText = $"{nameof(MosquePrayerTimeViewModel.PrayerTimesSet)}.{nameof(MosquePrayerTimesSet.Asr)}.{nameof(MosquePrayerTime.Start)}";
        addPrayerTimeUI(mainGrid, "Asr", durationBinding: bindingText,
            startRowNo: startRowNo + 4, startColumnNo: 3);

        bindingText = $"{nameof(MosquePrayerTimeViewModel.PrayerTimesSet)}.{nameof(MosquePrayerTimesSet.Maghrib)}.{nameof(MosquePrayerTime.Start)}";
        addPrayerTimeUI(mainGrid, "Maghrib", durationBinding: bindingText,
            startRowNo: startRowNo + 8, startColumnNo: 0);

        bindingText = $"{nameof(MosquePrayerTimeViewModel.PrayerTimesSet)}.{nameof(MosquePrayerTimesSet.Isha)}.{nameof(MosquePrayerTime.Start)}";
        addPrayerTimeUI(mainGrid, "Isha", durationBinding: bindingText,
            startRowNo: startRowNo + 8, startColumnNo: 3);

        return mainGrid;
    }

    private static void addPrayerTimeUI(
        Grid grid,
        string prayerName,
        string durationBinding,
        int startRowNo, int startColumnNo,
        string subtime1Binding = null, string subtime1Name = null,
        string subtime2Binding = null, string subtime2Name = null)
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

        if (!string.IsNullOrEmpty(subtime1Binding))
        {
            var subtime1Label = new Label
            {
                Text = subtime1Name,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };

            var subtime1DisplayText = new Label
            {
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            subtime1DisplayText.SetBinding(Label.TextProperty, new Binding(subtime1Binding, stringFormat: "{0:HH:mm:ss}"));

            grid.AddWithSpan(subtime1Label, startRowNo + 2, startColumnNo);
            grid.AddWithSpan(subtime1DisplayText, startRowNo + 2, startColumnNo + 1);

            subTimeTextViews.Add(subtime1Label);
            subTimeDisplayTextViews.Add(subtime1DisplayText);
        }

        if (!string.IsNullOrEmpty(subtime2Binding))
        {
            var subtime2Label = new Label
            {
                Text = subtime2Name,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            };

            var subtime2DisplayText = new Label
            {
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start
            };

            subtime2DisplayText.SetBinding(Label.TextProperty, new Binding(subtime2Binding, stringFormat: "{0:HH:mm:ss}"));

            grid.AddWithSpan(subtime2Label, startRowNo + 3, startColumnNo);
            grid.AddWithSpan(subtime2DisplayText, startRowNo + 3, startColumnNo + 1);

            subTimeTextViews.Add(subtime2Label);
            subTimeDisplayTextViews.Add(subtime2DisplayText);
        }

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
