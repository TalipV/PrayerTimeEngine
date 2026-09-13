using CommunityToolkit.Maui.Markup;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.Models.PrayerTimes;
using PrayerTimeEngine.Presentation.Pages.Main;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace PrayerTimeEngine.Presentation.Views.PrayerTimes;

public partial class DynamicPrayerTimeView : ContentView
{
    #region layout description

    private sealed record SubTimeDefinition(string Name, string Binding, string? ShowBinding = null);

    private sealed record BlockDefinition(
        string Name,
        ETimeSection Section,
        string Binding,
        SubTimeDefinition[] SubTimes,
        bool ShowOnlyStartTime = false);

    /// <summary>
    /// The whole view is derived from these two columns, so adding or removing a time
    /// is a change to the description and never to row or column indices.
    /// </summary>
    private static BlockDefinition[][] getColumns() =>
    [
        [
            new BlockDefinition("Fajr", ETimeSection.Fajr, currentDayBinding(nameof(DynamicPrayerTimesDay.Fajr)),
            [
                new SubTimeDefinition("Ghalas", nameof(FajrPrayerTime.Ghalas), nameof(DynamicPrayerTimeViewModel.ShowFajrGhalas)),
                new SubTimeDefinition("Redness", nameof(FajrPrayerTime.Karaha), nameof(DynamicPrayerTimeViewModel.ShowFajrRedness)),
            ]),
            new BlockDefinition("Dhuhr", ETimeSection.Dhuhr, currentDayBinding(nameof(DynamicPrayerTimesDay.Dhuhr)), []),
            new BlockDefinition("Maghrib", ETimeSection.Maghrib, currentDayBinding(nameof(DynamicPrayerTimesDay.Maghrib)),
            [
                new SubTimeDefinition("Sufficient", nameof(MaghribPrayerTime.SufficientTime), nameof(DynamicPrayerTimeViewModel.ShowMaghribSufficientTime)),
                new SubTimeDefinition("Ishtibak", nameof(MaghribPrayerTime.Ishtibak), nameof(DynamicPrayerTimeViewModel.ShowIshtibak)),
            ]),
        ],
        [
            new BlockDefinition("Duha", ETimeSection.Duha, currentDayBinding(nameof(DynamicPrayerTimesDay.Duha)),
            [
                new SubTimeDefinition("Quarter", nameof(DuhaPrayerTime.QuarterOfDay)),
                new SubTimeDefinition("Half*", nameof(DuhaPrayerTime.HalfOfDay)),
            ]),
            new BlockDefinition("Asr", ETimeSection.Asr, currentDayBinding(nameof(DynamicPrayerTimesDay.Asr)),
            [
                new SubTimeDefinition("Mithlayn", nameof(AsrPrayerTime.Mithlayn), nameof(DynamicPrayerTimeViewModel.ShowMithlayn)),
                new SubTimeDefinition("Karaha", nameof(AsrPrayerTime.Karaha), nameof(DynamicPrayerTimeViewModel.ShowKaraha)),
            ]),
            new BlockDefinition("Isha", ETimeSection.Isha, currentDayBinding(nameof(DynamicPrayerTimesDay.Isha)),
            [
                new SubTimeDefinition("1/3", nameof(IshaPrayerTime.FirstThirdOfNight)),
                new SubTimeDefinition("1/2", nameof(IshaPrayerTime.MiddleOfNight)),
                new SubTimeDefinition("2/3", nameof(IshaPrayerTime.SecondThirdOfNight)),
            ]),
        ],
    ];

    /// <summary>
    /// A single moment instead of a range, and not tied to one section of the day, so it gets
    /// its own row below both columns instead of a place in the prayer order.
    /// </summary>
    private static BlockDefinition getMomentBlock()
        => new("Qibla", ETimeSection.General, currentDayBinding(nameof(DynamicPrayerTimesDay.Qibla)), [], ShowOnlyStartTime: true);

    private static string currentDayBinding(string prayerTimeProperty)
        => $"{nameof(DynamicPrayerTimesDaySet.CurrentDay)}.{prayerTimeProperty}";

    #endregion layout description

    #region type scale

    // Only the proportions between the text roles are fixed, the absolute size is measured.
    // The prayer name is the reference, everything else is expressed relative to it.
    private const double PRAYER_TIME_RATIO = 0.75;
    private const double SUB_TIME_RATIO = 0.75;

    // A text line needs a bit more room than its font size.
    private const double LINE_HEIGHT_RATIO = 1.2;

    // Widths in multiples of the font size. Digits are the widest glyphs here, while the colons,
    // the dash and the spaces of a time are markedly narrower, so a plain character count would
    // overestimate a time by roughly a third and shrink the whole view for no reason.
    private const double TIME_RANGE_EM_WIDTH = 8.8;  // "00:00:00 - 00:00:00"
    private const double TIME_EM_WIDTH = 4.0;        // "00:00:00"
    private const double LETTER_EM_WIDTH = 0.5;      // average of a lower case name like "Sufficient"
    private const double SUB_TIME_GAP_EM_WIDTH = 1.0;

    // gap between two rows, as a share of the prayer name size, so the rows stay apart even when
    // there is no unused height left to distribute
    private const double MIN_ROW_GAP_RATIO = 0.9;

    // how much of the unused height goes into the gaps on top of that, the rest stays at the bottom
    private const double ROW_SPACING_SHARE = 0.75;

    private readonly List<(Label Label, double Ratio)> _scaledLabels = [];

    /// <summary>
    /// Height each column needs expressed in multiples of the prayer name size, so that the
    /// column which needs the most decides the scale and both columns stay aligned.
    /// </summary>
    private double _requiredLineUnits;

    /// <summary>
    /// Width one column needs expressed in multiples of the prayer name size.
    /// </summary>
    private double _requiredEmWidth;

    private int _blockRowCount;

    private double _lastAppliedFontSize;

    #endregion type scale

    private readonly MainPageViewModel _mainPageViewModel;
    private readonly ISystemInfoService _systemInfoService;

    public DynamicPrayerTimeView(MainPageViewModel mainPageViewModel, ISystemInfoService systemInfoService)
    {
        _mainPageViewModel = mainPageViewModel;
        _systemInfoService = systemInfoService;
        Content = createUI();
    }

    /// <summary>
    /// Both columns live in the same grid, so a row is exactly as high as the taller of its two
    /// blocks and the pairs stay on one line no matter how many sub times they have.
    /// </summary>
    private View createUI()
    {
        BlockDefinition[][] columns = getColumns();
        BlockDefinition momentBlock = getMomentBlock();

        _blockRowCount = columns.Max(column => column.Length);

        // the star row soaks up whatever is left, which keeps the moment row at the bottom
        GridLength[] rows = [.. Enumerable.Repeat(Auto, _blockRowCount), Star, Auto];

        var mainGrid = new Grid
        {
            Padding = new Thickness(10, 14, 10, 10),
            ColumnSpacing = 14,
            RowDefinitions = Rows.Define(rows),
            ColumnDefinitions = Columns.Define(Star, Star)
        };

        for (int columnIndex = 0; columnIndex < columns.Length; columnIndex++)
        {
            BlockDefinition[] blocks = columns[columnIndex];

            for (int rowIndex = 0; rowIndex < blocks.Length; rowIndex++)
            {
                mainGrid.Add(createBlock(blocks[rowIndex]), column: columnIndex, row: rowIndex);
            }
        }

        mainGrid.AddWithSpan(createMomentRow(momentBlock), row: _blockRowCount + 1, column: 0, columnSpan: 2);

        // the moment row is a single line, as high as the larger of its two labels, and every row
        // is followed by a gap which has to be part of the height the view asks for
        _requiredLineUnits =
            columns.Max(getRequiredLineUnits)
            + 1.0
            + (_blockRowCount + 1) * MIN_ROW_GAP_RATIO;
        _requiredEmWidth = columns.Max(getRequiredEmWidth);
        mainGrid.SizeChanged += (_, _) => applyTypeScale(mainGrid);

        return mainGrid;
    }

    private View createBlock(BlockDefinition block)
    {
        var blockLayout = new VerticalStackLayout { VerticalOptions = LayoutOptions.Start };

        var prayerNameLabel = new Label
        {
            Text = block.Name,
            TextColor = AppColors.Text,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Start
        };

        prayerNameLabel.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = _mainPageViewModel.GoToSettingsPageCommand,
            CommandParameter = block.Section
        });

        Label prayerDurationLabel = createTimeLabel(block);

        blockLayout.Add(prayerNameLabel);
        blockLayout.Add(prayerDurationLabel);

        _scaledLabels.Add((prayerNameLabel, 1.0));
        _scaledLabels.Add((prayerDurationLabel, PRAYER_TIME_RATIO));

        foreach (SubTimeDefinition subTime in block.SubTimes)
        {
            blockLayout.Add(createSubTime(block, subTime));
        }

        return blockLayout;
    }

    private Label createTimeLabel(BlockDefinition block)
    {
        var timeLabel = new Label
        {
            TextColor = AppColors.Text,
            HorizontalOptions = LayoutOptions.Start
        };

        timeLabel.Bind(
            Label.TextProperty,
            $"{nameof(DynamicPrayerTimeViewModel.PrayerTimesSet)}.{block.Binding}",
            convert: (GenericPrayerTime? prayerTime) =>
            {
                ZonedDateTime? startDisplayValue = _systemInfoService.GetInCurrentZone(prayerTime?.Start);
                string startTime = startDisplayValue?.ToString("HH:mm:ss", null) ?? "xx:xx:xx";

                if (block.ShowOnlyStartTime)
                {
                    return startTime;
                }

                ZonedDateTime? endDisplayValue = _systemInfoService.GetInCurrentZone(prayerTime?.End);
                string endTime = endDisplayValue?.ToString("HH:mm:ss", null) ?? "xx:xx:xx";

                return $"{startTime} - {endTime}";
            });

        return timeLabel;
    }

    /// <summary>
    /// Name and value on one line, so the row stays flat and does not compete with the prayers.
    /// </summary>
    private View createMomentRow(BlockDefinition block)
    {
        var nameLabel = new Label
        {
            Text = block.Name,
            TextColor = AppColors.Text,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };

        nameLabel.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = _mainPageViewModel.GoToSettingsPageCommand,
            CommandParameter = block.Section
        });

        Label timeLabel = createTimeLabel(block);
        timeLabel.VerticalOptions = LayoutOptions.Center;

        // same sizes as a prayer and its time, the row only differs in being on one line
        _scaledLabels.Add((nameLabel, 1.0));
        _scaledLabels.Add((timeLabel, PRAYER_TIME_RATIO));

        return new HorizontalStackLayout
        {
            Spacing = 10,
            Children = { nameLabel, timeLabel }
        };
    }

    private View createSubTime(BlockDefinition block, SubTimeDefinition subTime)
    {
        // the name takes what it needs, the value keeps to the right edge so the values line up
        var subTimeGrid = new Grid
        {
            ColumnDefinitions = Columns.Define(Auto, Star)
        };

        var nameLabel = new Label
        {
            Text = subTime.Name,
            TextColor = AppColors.Text,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center
        };

        var valueLabel = new Label
        {
            TextColor = AppColors.Text,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center
        };

        valueLabel.Bind(
            Label.TextProperty,
            $"{nameof(DynamicPrayerTimeViewModel.PrayerTimesSet)}.{block.Binding}.{subTime.Binding}",
            convert: (ZonedDateTime? value) => _systemInfoService.GetInCurrentZone(value),
            stringFormat: "{0:HH:mm:ss}");

        if (!string.IsNullOrEmpty(subTime.ShowBinding))
        {
            subTimeGrid.SetBinding(IsVisibleProperty, subTime.ShowBinding);
        }

        subTimeGrid.Add(nameLabel, column: 0);
        subTimeGrid.Add(valueLabel, column: 1);

        _scaledLabels.Add((nameLabel, SUB_TIME_RATIO));
        _scaledLabels.Add((valueLabel, SUB_TIME_RATIO));

        return subTimeGrid;
    }

    private static double getRequiredLineUnits(BlockDefinition[] blocks)
        => blocks.Sum(block => 1.0 + PRAYER_TIME_RATIO + block.SubTimes.Length * SUB_TIME_RATIO);

    /// <summary>
    /// The widest line of a column is either a full time range or the longest sub time line,
    /// which is its name and its value next to each other.
    /// </summary>
    private static double getRequiredEmWidth(BlockDefinition[] blocks)
    {
        double timeRangeWidth = TIME_RANGE_EM_WIDTH * PRAYER_TIME_RATIO;

        int longestSubTimeName = blocks
            .SelectMany(block => block.SubTimes)
            .Select(subTime => subTime.Name.Length)
            .DefaultIfEmpty(0)
            .Max();

        double subTimeWidth =
            (longestSubTimeName * LETTER_EM_WIDTH + SUB_TIME_GAP_EM_WIDTH + TIME_EM_WIDTH) * SUB_TIME_RATIO;

        return Math.Max(timeRangeWidth, subTimeWidth);
    }

    /// <summary>
    /// Derives the font size from the space the view actually got: large enough to fill the
    /// height, small enough that a full time range still fits into one column.
    /// </summary>
    private void applyTypeScale(Grid mainGrid)
    {
        double height = mainGrid.Height - mainGrid.Padding.VerticalThickness;
        double width = mainGrid.Width - mainGrid.Padding.HorizontalThickness - mainGrid.ColumnSpacing;

        if (width <= 0 || height <= 0 || _requiredLineUnits <= 0)
            return;

        double fontSizeByHeight = height / (_requiredLineUnits * LINE_HEIGHT_RATIO);

        double columnWidth = width / 2;
        double fontSizeByWidth = columnWidth / _requiredEmWidth;

        double fontSize = Math.Min(fontSizeByHeight, fontSizeByWidth);

        // resizing the labels changes the layout again, so ignore the resulting echo
        if (Math.Abs(fontSize - _lastAppliedFontSize) < 0.5)
            return;

        _lastAppliedFontSize = fontSize;

        foreach ((Label label, double ratio) in _scaledLabels)
        {
            label.FontSize = fontSize * ratio;
        }

        applyRowSpacing(mainGrid, height, fontSize);
    }

    /// <summary>
    /// When the width limits the font size, the text does not fill the height. Most of that
    /// remainder is spent on the gaps between the rows, the rest lands in the star row and
    /// therefore between the last prayer and the moment row.
    /// </summary>
    private void applyRowSpacing(Grid mainGrid, double height, double fontSize)
    {
        double neededHeight = fontSize * _requiredLineUnits * LINE_HEIGHT_RATIO;
        double leftoverHeight = Math.Max(0, height - neededHeight);

        // one gap per block row plus the one in front of the moment row
        double distributedGap = leftoverHeight / (_blockRowCount + 1) * ROW_SPACING_SHARE;

        mainGrid.RowSpacing = fontSize * MIN_ROW_GAP_RATIO + distributedGap;
    }
}
