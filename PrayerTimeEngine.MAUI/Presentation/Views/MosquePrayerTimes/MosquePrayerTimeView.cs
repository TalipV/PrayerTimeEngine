using CommunityToolkit.Maui.Markup;
using NodaTime;
using OnScreenSizeMarkup.Maui.Helpers;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace PrayerTimeEngine.Presentation.Views.MosquePrayerTimes;

public partial class MosquePrayerTimeView : ContentView
{
    private readonly ISystemInfoService _systemInfoService;

    public MosquePrayerTimeView(ISystemInfoService systemInfoService)
    {
        _systemInfoService = systemInfoService;
        Content = createUI();
    }

    private Grid createUI()
    {
        var mainGrid = new Grid
        {
            RowDefinitions = Rows.Define(
                new GridLength(2, GridUnitType.Star),
                new GridLength(2, GridUnitType.Star),
                GridLength.Star,
                GridLength.Star,
                GridLength.Star,
                new GridLength(2, GridUnitType.Star),
                new GridLength(2, GridUnitType.Star),
                GridLength.Star,
                GridLength.Star,
                new GridLength(2, GridUnitType.Star),
                new GridLength(2, GridUnitType.Star),
                GridLength.Star,
                GridLength.Star,
                GridLength.Star,
                GridLength.Star
            ),
            ColumnDefinitions = Columns.Define(
                new GridLength(3, GridUnitType.Star),
                new GridLength(4, GridUnitType.Star),
                new GridLength(2, GridUnitType.Star),
                new GridLength(3, GridUnitType.Star),
                new GridLength(4, GridUnitType.Star)
            )
        }
        .Paddings(10, 20, 10, 20);

        int startRowNo = 1;

        addPrayerTimeUI(mainGrid, "Fajr", nameof(MosquePrayerTimesDay.Fajr),
            startRowNo, startColumnNo: 0);
        addPrayerTimeUI(mainGrid, "Jumu'ah", nameof(MosquePrayerTimesDay.Jumuah),
            startRowNo, startColumnNo: 3,
            subtime1Name: "Jumuah2", subtime1Binding: $"{nameof(MosquePrayerTimesDay.Jumuah2)}.{nameof(GenericPrayerTime.Start)}");
        addPrayerTimeUI(mainGrid, "Dhuhr", nameof(MosquePrayerTimesDay.Dhuhr),
            startRowNo + 4, startColumnNo: 0);
        addPrayerTimeUI(mainGrid, "Asr", nameof(MosquePrayerTimesDay.Asr),
            startRowNo + 4, startColumnNo: 3);
        addPrayerTimeUI(mainGrid, "Maghrib", nameof(MosquePrayerTimesDay.Maghrib),
            startRowNo + 8, startColumnNo: 0);
        addPrayerTimeUI(mainGrid, "Isha", nameof(MosquePrayerTimesDay.Isha),
            startRowNo + 8, startColumnNo: 3);

        return mainGrid;
    }

    private void addPrayerTimeUI(
        Grid grid,
        string prayerName,
        string bindingText,
        int startRowNo, int startColumnNo,
        string subtime1Binding = null, string subtime1Name = null)
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
        prayerDurationLabel.Bind(
            Label.TextProperty,
            $"{nameof(MosquePrayerTimeViewModel.PrayerTimesSet)}.{bindingText}",
            convert: (MosquePrayerTime prayerTime) =>
            {
                if (prayerTime.Start == null)
                    return "xx:xx";

                ZonedDateTime? prayerTimeStartDisplayValue = _systemInfoService.GetInCurrentZone(prayerTime.Start);
                ZonedDateTime? prayerTimeEndDisplayValue = _systemInfoService.GetInCurrentZone(prayerTime.End);

                string startTime = prayerTimeStartDisplayValue?.ToString("HH:mm", null) ?? "xx.xx";
                string endTime = prayerTimeEndDisplayValue?.ToString("HH:mm", null) ?? "xx.xx";
                string congregationTime = prayerTime.CongregationStartOffset > 0
                    ? prayerTime.Start?.PlusMinutes(prayerTime.CongregationStartOffset).ToString("HH:mm", null) ?? "xx.xx"
                    : "";

                string fullTimeText = startTime;

                // TODO improve
                if (prayerName == "Fajr")
                {
                    fullTimeText += $"-{endTime}";
                }

                if (!string.IsNullOrWhiteSpace(congregationTime))
                {
                    fullTimeText += $" ({congregationTime})";
                }

                return fullTimeText;
            });

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
            subtime1DisplayText.Bind(
                Label.TextProperty, 
                $"{nameof(MosquePrayerTimeViewModel.PrayerTimesSet)}.{subtime1Binding}",
                convert: (ZonedDateTime? subTime1) =>
                {
                    return _systemInfoService.GetInCurrentZone(subTime1);
                },
                stringFormat: "{0:HH:mm:ss}");

            grid.AddWithSpan(subtime1Label, startRowNo + 2, startColumnNo);
            grid.AddWithSpan(subtime1DisplayText, startRowNo + 2, startColumnNo + 1);

            subTimeTextViews.Add(subtime1Label);
            subTimeDisplayTextViews.Add(subtime1DisplayText);
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
