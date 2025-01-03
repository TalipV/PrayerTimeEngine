using CommunityToolkit.Maui.Markup;
using OnScreenSizeMarkup.Maui.Helpers;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.Models.PrayerTimes;
using PrayerTimeEngine.Presentation.Pages.Main;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace PrayerTimeEngine.Presentation.Views.PrayerTimes;

public partial class DynamicPrayerTimeView : ContentView
{
    private readonly MainPageViewModel _mainPageViewModel;

    public DynamicPrayerTimeView(MainPageViewModel mainPageViewModel)
    {
        _mainPageViewModel = mainPageViewModel;
        Content = createUI();
    }

    private Grid createUI()
    {
        var mainGrid = new Grid
        {
            Padding = new Thickness(10, 20, 10, 20),
            RowDefinitions = Rows.Define(
                new GridLength(2, GridUnitType.Star),
                new GridLength(2, GridUnitType.Star),
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
        };

        int startRowNo = 1;

        addPrayerTimeUI(mainGrid, "Fajr", nameof(DynamicPrayerTimesSet.Fajr),
            startRowNo, startColumnNo: 0,
            subtime1Name: "Ghalas", showSubtime1Binding: $"{nameof(DynamicPrayerTimeViewModel.ShowFajrGhalas)}", subtime1Binding: nameof(FajrPrayerTime.Ghalas),
            subtime2Name: "Redness", showSubtime2Binding: $"{nameof(DynamicPrayerTimeViewModel.ShowFajrRedness)}", subtime2Binding: nameof(FajrPrayerTime.Karaha));

        addPrayerTimeUI(mainGrid, "Duha", nameof(DynamicPrayerTimesSet.Duha),
            startRowNo, startColumnNo: 3,
            subtime1Name: "Quarter", subtime1Binding: $"{nameof(DynamicPrayerTimesSet.Duha)}.{nameof(DuhaPrayerTime.QuarterOfDay)}");

        addPrayerTimeUI(mainGrid, "Dhuhr", nameof(DynamicPrayerTimesSet.Dhuhr),
            startRowNo + 4, startColumnNo: 0);

        addPrayerTimeUI(mainGrid, "Asr", nameof(DynamicPrayerTimesSet.Asr),
            startRowNo + 4, startColumnNo: 3,
            subtime1Name: "Mithlayn", showSubtime1Binding: $"{nameof(DynamicPrayerTimeViewModel.ShowMithlayn)}", subtime1Binding: nameof(AsrPrayerTime.Mithlayn),
            subtime2Name: "Karaha", showSubtime2Binding: $"{nameof(DynamicPrayerTimeViewModel.ShowKaraha)}", subtime2Binding: nameof(AsrPrayerTime.Karaha));

        addPrayerTimeUI(mainGrid, "Maghrib", nameof(DynamicPrayerTimesSet.Maghrib),
            startRowNo + 8, startColumnNo: 0,
            subtime1Name: "Sufficient", showSubtime1Binding: $"{nameof(DynamicPrayerTimeViewModel.ShowMaghribSufficientTime)}", subtime1Binding: nameof(MaghribPrayerTime.SufficientTime),
            subtime2Name: "Ishtibaq", showSubtime2Binding: $"{nameof(DynamicPrayerTimeViewModel.ShowIshtibaq)}", subtime2Binding: nameof(MaghribPrayerTime.Ishtibaq));

        addPrayerTimeUI(mainGrid, "Isha", nameof(DynamicPrayerTimesSet.Isha),
            startRowNo + 8, startColumnNo: 3,
            subtime1Name: "1/3", subtime1Binding: nameof(IshaPrayerTime.FirstThirdOfNight),
            subtime2Name: "1/2", subtime2Binding: nameof(IshaPrayerTime.MiddleOfNight),
            subtime3Name: "2/3", subtime3Binding: nameof(IshaPrayerTime.SecondThirdOfNight));

        return mainGrid;
    }

    private void addPrayerTimeUI(
        Grid grid,
        string prayerName,
        string bindingText,
        int startRowNo, int startColumnNo,
        string subtime1Name = null, string showSubtime1Binding = null, string subtime1Binding = null,
        string subtime2Name = null, string showSubtime2Binding = null, string subtime2Binding = null,
        string showSubtime3Binding = null, string subtime3Name = null, string subtime3Binding = null)
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

        prayerNameLabel.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = _mainPageViewModel.GoToSettingsPageCommand,
            CommandParameter = Enum.Parse<EPrayerType>(prayerName)
        });
        grid.AddWithSpan(prayerNameLabel, startRowNo, startColumnNo, columnSpan: 2);

        var prayerDurationLabel = new Label
        {
            TextColor = Colors.Black,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start
        };
        prayerDurationLabel.Bind(
            Label.TextProperty,
            $"{nameof(DynamicPrayerTimeViewModel.PrayerTimesSet)}.{bindingText}",
            convert: (GenericPrayerTime prayerTime) =>
            {
                string startTime = prayerTime.Start?.ToString("HH:mm:ss", null) ?? "xx:xx:xx";
                string endTime = prayerTime.End?.ToString("HH:mm:ss", null) ?? "xx:xx:xx";

                return $"{startTime} - {endTime}";
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

            if (!string.IsNullOrEmpty(showSubtime1Binding))
            {
                subtime1Label.SetBinding(IsVisibleProperty, showSubtime1Binding);
            }

            var subtime1DisplayText = new Label
            {
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            subtime1DisplayText.SetBinding(Label.TextProperty, $"{nameof(DynamicPrayerTimeViewModel.PrayerTimesSet)}.{bindingText}.{subtime1Binding}", stringFormat: "{0:HH:mm:ss}");
            if (!string.IsNullOrEmpty(showSubtime1Binding))
            {
                subtime1DisplayText.SetBinding(IsVisibleProperty, showSubtime1Binding);
            }

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
            if (!string.IsNullOrEmpty(showSubtime2Binding))
            {
                subtime2Label.SetBinding(IsVisibleProperty, showSubtime2Binding);
            }

            var subtime2DisplayText = new Label
            {
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start
            };

            subtime2DisplayText.SetBinding(Label.TextProperty, $"{nameof(DynamicPrayerTimeViewModel.PrayerTimesSet)}.{bindingText}.{subtime2Binding}", stringFormat: "{0:HH:mm:ss}");

            if (!string.IsNullOrEmpty(showSubtime2Binding))
            {
                subtime2DisplayText.SetBinding(IsVisibleProperty, showSubtime2Binding);
            }

            grid.AddWithSpan(subtime2Label, startRowNo + 3, startColumnNo);
            grid.AddWithSpan(subtime2DisplayText, startRowNo + 3, startColumnNo + 1);

            subTimeTextViews.Add(subtime2Label);
            subTimeDisplayTextViews.Add(subtime2DisplayText);
        }

        if (!string.IsNullOrEmpty(subtime3Binding))
        {
            var subtime3Label = new Label
            {
                Text = subtime3Name,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            };
            if (!string.IsNullOrEmpty(showSubtime3Binding))
            {
                subtime3Label.SetBinding(IsVisibleProperty, showSubtime3Binding);
            }

            var subtime3DisplayText = new Label
            {
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start
            };

            subtime3DisplayText.SetBinding(Label.TextProperty, $"{nameof(DynamicPrayerTimeViewModel.PrayerTimesSet)}.{bindingText}.{subtime3Binding}", stringFormat: "{0:HH:mm:ss}");

            if (!string.IsNullOrEmpty(showSubtime3Binding))
            {
                subtime3DisplayText.SetBinding(IsVisibleProperty, showSubtime3Binding);
            }

            grid.AddWithSpan(subtime3Label, startRowNo + 4, startColumnNo);
            grid.AddWithSpan(subtime3DisplayText, startRowNo + 4, startColumnNo + 1);

            subTimeTextViews.Add(subtime3Label);
            subTimeDisplayTextViews.Add(subtime3DisplayText);
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
