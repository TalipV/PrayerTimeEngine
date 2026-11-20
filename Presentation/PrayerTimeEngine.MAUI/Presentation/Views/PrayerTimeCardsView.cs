using System.Windows.Input;
using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.Shapes;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace PrayerTimeEngine.Presentation.Views;

/// <summary>
/// Shared look of the prayer time pages: every prayer sits on its own card, the cards are laid out
/// in two columns and the font size is derived from the space the view got. The derived views only
/// describe which times are shown and how they are formatted, so all pages look the same.
/// </summary>
public abstract class PrayerTimeCardsView(ISystemInfoService systemInfoService) : ContentView
{
    #region layout description

    /// <summary>
    /// A secondary time shown as its own line below the main time of a block.
    /// </summary>
    /// <param name="Binding">Path relative to the binding context of the view.</param>
    /// <param name="Format">Turns the bound value into the displayed text.</param>
    /// <param name="ShowBinding">Optional path whose value decides whether the line is shown.</param>
    /// <param name="ShowConverter">Turns the value of <paramref name="ShowBinding"/> into the visibility, if it is no bool itself.</param>
    protected sealed record SubTimeDefinition(
        string Name,
        string Binding,
        Func<object?, string> Format,
        string? ShowBinding = null,
        Func<object?, bool>? ShowConverter = null);

    /// <summary>
    /// A second time on the line of the main time, right-aligned, e.g. a second Friday prayer.
    /// </summary>
    protected sealed record SecondaryTimeDefinition(string Binding, Func<object?, string> Format);

    /// <param name="Binding">Path of the main time relative to the binding context of the view.</param>
    /// <param name="Format">Turns the bound value into the displayed main time.</param>
    protected sealed record BlockDefinition(
        string Name,
        ETimeSection Section,
        string Binding,
        Func<object?, string> Format,
        SubTimeDefinition[] SubTimes,
        SecondaryTimeDefinition? SecondaryTime = null);

    /// <summary>
    /// The whole view is derived from these columns, so adding or removing a time is a change to
    /// the description and never to row or column indices.
    /// </summary>
    protected abstract BlockDefinition[][] GetColumns();

    /// <summary>
    /// A single moment which is not part of the prayer order. It gets its own row below both
    /// columns, with name and value on one line.
    /// </summary>
    protected virtual BlockDefinition? GetMomentBlock() => null;

    /// <summary>
    /// Width of the widest main time in multiples of its font size, e.g. a full time range.
    /// </summary>
    protected abstract double MainTimeEmWidth { get; }

    /// <summary>
    /// Width of a sub time value in multiples of its font size.
    /// </summary>
    protected abstract double SubTimeEmWidth { get; }

    /// <summary>
    /// Executed with the section of a block when its card is tapped. No command, no tap.
    /// </summary>
    protected virtual ICommand? CardTapCommand => null;

    /// <summary>
    /// The prayer times shown on the cards, out of the bound prayer times set.
    /// </summary>
    protected abstract List<(ETimeSection Section, GenericPrayerTime Times)>? GetDisplayedPrayerTimes(IPrayerTimesDay prayerTimesSet);

    #endregion layout description

    #region type scale

    // Only the proportions between the text roles are fixed, the absolute size is measured.
    // The prayer name is the reference, everything else is expressed relative to it.
    protected const double PrayerTimeRatio = 0.75;
    protected const double SubTimeRatio = 0.75;

    // A text line needs a bit more room than its font size.
    private const double LineHeightRatio = 1.2;

    // Widths in multiples of the font size.
    private const double LetterEmWidth = 0.5;      // average of a lower case name like "Sufficient"
    private const double SubTimeGapEmWidth = 1.0;

    // gap between two rows, as a share of the prayer name size, so the rows stay apart even when
    // there is no unused height left to distribute. Small, because the cards already separate the rows.
    private const double MinRowGapRatio = 0.4;

    // every block sits on its own card, whose padding is fixed and therefore not part of the type scale
    private const double CardPaddingHorizontal = 12;
    private const double CardPaddingVertical = 10;
    private const double CardCornerRadius = 14;
    private const double CardStrokeThickness = 1.5;

    // how much of the unused height goes into the gaps on top of that, the rest stays at the bottom
    private const double RowSpacingShare = 0.75;

    private readonly List<(Label Label, double Ratio)> _scaledLabels = [];

    /// <summary>
    /// Height the view needs expressed in multiples of the prayer name size. The column which
    /// needs the most decides the scale, so both columns stay aligned.
    /// </summary>
    private double _requiredLineUnits;

    /// <summary>
    /// Width one column needs expressed in multiples of the prayer name size.
    /// </summary>
    private double _requiredEmWidth;

    /// <summary>
    /// Rows with a card: the block rows plus the moment row, if there is one.
    /// </summary>
    private int _cardRowCount;

    private double _lastAppliedFontSize;

    #endregion type scale

    protected ISystemInfoService SystemInfoService { get; } = systemInfoService;

    /// <summary>
    /// Builds the UI. Called by the derived constructor, after its own fields are set, because the
    /// layout description may depend on them.
    /// </summary>
    protected void InitializeUI()
    {
        Content = CreateUI();
    }

    /// <summary>
    /// Both columns live in the same grid, so a row is exactly as high as the taller of its two
    /// blocks and the pairs stay on one line no matter how many sub times they have.
    /// </summary>
    private Grid CreateUI()
    {
        BlockDefinition[][] columns = GetColumns();
        BlockDefinition? momentBlock = GetMomentBlock();

        int blockRowCount = columns.Max(column => column.Length);
        _cardRowCount = blockRowCount + (momentBlock is null ? 0 : 1);

        // the star row soaks up whatever is left, which keeps the moment row at the bottom
        GridLength[] rows = momentBlock is null
            ? [.. Enumerable.Repeat(Auto, blockRowCount), Star]
            : [.. Enumerable.Repeat(Auto, blockRowCount), Star, Auto];

        var mainGrid = new Grid
        {
            Padding = new Thickness(0, 12, 0, 6),
            ColumnSpacing = 10,
            RowDefinitions = Rows.Define(rows),
            ColumnDefinitions = Columns.Define(Star, Star)
        };

        for (int columnIndex = 0; columnIndex < columns.Length; columnIndex++)
        {
            BlockDefinition[] blocks = columns[columnIndex];

            for (int rowIndex = 0; rowIndex < blocks.Length; rowIndex++)
            {
                mainGrid.Add(CreateBlock(blocks[rowIndex]), column: columnIndex, row: rowIndex);
            }
        }

        _requiredLineUnits = columns.Max(GetRequiredLineUnits) + (_cardRowCount * MinRowGapRatio);

        if (momentBlock is not null)
        {
            mainGrid.AddWithSpan(CreateMomentRow(momentBlock), row: blockRowCount + 1, column: 0, columnSpan: 2);

            // the moment row is a single line, as high as the larger of its two labels
            _requiredLineUnits += 1.0;
        }

        _requiredEmWidth = columns.Max(GetRequiredEmWidth);
        mainGrid.SizeChanged += (_, _) => ApplyTypeScale(mainGrid);

        return mainGrid;
    }

    private Border CreateBlock(BlockDefinition block)
    {
        var blockLayout = new VerticalStackLayout { VerticalOptions = LayoutOptions.Start };

        var prayerNameLabel = new Label
        {
            Text = block.Name,
            TextColor = AppColors.Text,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Start
        };

        Label prayerDurationLabel = CreateTimeLabel(block.Binding, block.Format);

        blockLayout.Add(prayerNameLabel);
        blockLayout.Add(CreateTimeLine(prayerDurationLabel, block.SecondaryTime));

        _scaledLabels.Add((prayerNameLabel, 1.0));
        _scaledLabels.Add((prayerDurationLabel, PrayerTimeRatio));

        foreach (SubTimeDefinition subTime in block.SubTimes)
        {
            blockLayout.Add(CreateSubTime(subTime));
        }

        Border card = CreateCard(block, blockLayout);
        // the running prayer only sits on a lighter card: a quiet hint instead of a coloured frame
        BindToCurrentPrayer(card, BackgroundColorProperty, block, isCurrent => isCurrent ? AppColors.Surface : AppColors.Card);
        BindToCurrentPrayer(card, Border.StrokeProperty, block, isCurrent => new SolidColorBrush(isCurrent ? AppColors.Surface : AppColors.Card));

        return card;
    }

    /// <summary>
    /// The card around a block. The whole card is the tap target, which is much larger than the
    /// prayer name alone.
    /// </summary>
    private Border CreateCard(BlockDefinition block, View content)
    {
        var card = new Border
        {
            BackgroundColor = AppColors.Card,
            Stroke = AppColors.Card,
            StrokeThickness = CardStrokeThickness,
            StrokeShape = new RoundRectangle { CornerRadius = CardCornerRadius },
            Padding = new Thickness(CardPaddingHorizontal, CardPaddingVertical),
            Content = content
        };

        if (CardTapCommand is ICommand tapCommand)
        {
            card.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = tapCommand,
                CommandParameter = block.Section
            });
        }

        return card;
    }

    /// <summary>
    /// Highlights the prayer whose time is running right now or, in a gap between two prayer times,
    /// the next one; the same prayer the graphic shows. Evaluated whenever the times are
    /// refreshed, which also happens when the app comes back to the foreground.
    /// </summary>
    private void BindToCurrentPrayer<TValue>(BindableObject target, BindableProperty property, BlockDefinition block, Func<bool, TValue> valueSelector)
    {
        target.SetBinding(
            property,
            new Binding(
                nameof(IPrayerTimeViewModel.PrayerTimesSet),
                converter: new FuncConverter<IPrayerTimesDay?, TValue>(prayerTimesSet => valueSelector(IsCurrentPrayer(prayerTimesSet, block)))));
    }

    private bool IsCurrentPrayer(IPrayerTimesDay? prayerTimesSet, BlockDefinition block)
    {
        if (prayerTimesSet is null || GetDisplayedPrayerTimes(prayerTimesSet) is not { } displayedPrayerTimes)
        {
            return false;
        }

        GenericPrayerTime? highlightedPrayerTime = prayerTimesSet.GetCurrentOrNextPrayerTime(SystemInfoService.GetCurrentInstant()).Times;

        // compared by reference: a set may also hold the previous and the next day, whose prayer
        // times are not the ones shown on the cards
        return highlightedPrayerTime is not null
            && displayedPrayerTimes.Any(x => x.Section == block.Section && ReferenceEquals(x.Times, highlightedPrayerTime));
    }

    private static Label CreateTimeLabel(string binding, Func<object?, string> format)
    {
        var timeLabel = new Label
        {
            TextColor = AppColors.Text,
            HorizontalOptions = LayoutOptions.Start,
            LineBreakMode = LineBreakMode.NoWrap
        };

        timeLabel.SetBinding(
            Label.TextProperty,
            new Binding(binding, converter: new FuncConverter<object?, string>(format)));

        return timeLabel;
    }

    /// <summary>
    /// The main time, with the secondary time, if any, on the same line at the right edge.
    /// </summary>
    private View CreateTimeLine(Label mainTimeLabel, SecondaryTimeDefinition? secondaryTime)
    {
        if (secondaryTime is null)
        {
            return mainTimeLabel;
        }

        Label secondaryTimeLabel = CreateTimeLabel(secondaryTime.Binding, secondaryTime.Format);
        secondaryTimeLabel.HorizontalOptions = LayoutOptions.End;
        _scaledLabels.Add((secondaryTimeLabel, PrayerTimeRatio));

        // both labels share one cell, so neither width depends on a measured column
        return new Grid { mainTimeLabel, secondaryTimeLabel };
    }

    /// <summary>
    /// Name and value on one line, so the row stays flat and does not compete with the prayers.
    /// </summary>
    private Border CreateMomentRow(BlockDefinition block)
    {
        var nameLabel = new Label
        {
            Text = block.Name,
            TextColor = AppColors.Text,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };

        Label timeLabel = CreateTimeLabel(block.Binding, block.Format);
        timeLabel.VerticalOptions = LayoutOptions.Center;
        timeLabel.HorizontalOptions = LayoutOptions.End;

        // same sizes as a prayer and its time, the row only differs in being on one line
        _scaledLabels.Add((nameLabel, 1.0));
        _scaledLabels.Add((timeLabel, PrayerTimeRatio));

        var rowGrid = new Grid
        {
            ColumnDefinitions = Columns.Define(Auto, Star)
        };
        rowGrid.Add(nameLabel, column: 0);
        rowGrid.Add(timeLabel, column: 1);

        return CreateCard(block, rowGrid);
    }

    private Grid CreateSubTime(SubTimeDefinition subTime)
    {
        // Name and value share one cell, the name at the left and the value at the right edge so the
        // values line up. An Auto column for the name would keep the width it measured while the row
        // was hidden, which cuts the name off once the row appears with the scaled font size.
        var subTimeGrid = new Grid();

        var nameLabel = new Label
        {
            Text = subTime.Name,
            TextColor = AppColors.TextMuted,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.NoWrap
        };

        var valueLabel = new Label
        {
            TextColor = AppColors.Text,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.NoWrap
        };

        valueLabel.SetBinding(
            Label.TextProperty,
            new Binding(subTime.Binding, converter: new FuncConverter<object?, string>(subTime.Format)));

        if (!string.IsNullOrEmpty(subTime.ShowBinding))
        {
            subTimeGrid.SetBinding(
                IsVisibleProperty,
                subTime.ShowConverter is null
                    ? new Binding(subTime.ShowBinding)
                    : new Binding(subTime.ShowBinding, converter: new FuncConverter<object?, bool>(subTime.ShowConverter)));

            // a hidden row is not measured again when the font size changes, so do it when it appears
            subTimeGrid.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == IsVisibleProperty.PropertyName && subTimeGrid.IsVisible)
                {
                    ((IView)nameLabel).InvalidateMeasure();
                    ((IView)valueLabel).InvalidateMeasure();
                    ((IView)subTimeGrid).InvalidateMeasure();
                }
            };
        }

        subTimeGrid.Add(nameLabel);
        subTimeGrid.Add(valueLabel);

        _scaledLabels.Add((nameLabel, SubTimeRatio));
        _scaledLabels.Add((valueLabel, SubTimeRatio));

        return subTimeGrid;
    }

    private static double GetRequiredLineUnits(BlockDefinition[] blocks)
        => blocks.Sum(block => 1.0 + PrayerTimeRatio + (block.SubTimes.Length * SubTimeRatio));

    /// <summary>
    /// The widest line of a column is either the main time or the longest sub time line, which is
    /// its name and its value next to each other.
    /// </summary>
    private double GetRequiredEmWidth(BlockDefinition[] blocks)
    {
        double mainTimeWidth = MainTimeEmWidth * PrayerTimeRatio;

        int longestSubTimeName = blocks
            .SelectMany(block => block.SubTimes)
            .Select(subTime => subTime.Name.Length)
            .DefaultIfEmpty(0)
            .Max();

        double subTimeWidth =
            ((longestSubTimeName * LetterEmWidth) + SubTimeGapEmWidth + SubTimeEmWidth) * SubTimeRatio;

        return Math.Max(mainTimeWidth, subTimeWidth);
    }

    /// <summary>
    /// Derives the font size from the space the view actually got: large enough to fill the
    /// height, small enough that the widest time still fits into one column.
    /// </summary>
    private void ApplyTypeScale(Grid mainGrid)
    {
        // the card paddings of every row and of one column are fixed
        double cardHeight = _cardRowCount * GetCardVerticalThickness();
        double height = mainGrid.Height - mainGrid.Padding.VerticalThickness - cardHeight;
        double width = mainGrid.Width - mainGrid.Padding.HorizontalThickness - mainGrid.ColumnSpacing;

        if (width <= 0 || height <= 0 || _requiredLineUnits <= 0)
        {
            return;
        }

        double fontSizeByHeight = height / (_requiredLineUnits * LineHeightRatio);

        double columnWidth = (width / 2) - (2 * (CardPaddingHorizontal + CardStrokeThickness));
        double fontSizeByWidth = columnWidth / _requiredEmWidth;

        double fontSize = Math.Min(fontSizeByHeight, fontSizeByWidth);

        // resizing the labels changes the layout again, so ignore the resulting echo
        if (Math.Abs(fontSize - _lastAppliedFontSize) < 0.5)
        {
            return;
        }

        _lastAppliedFontSize = fontSize;

        foreach ((Label label, double ratio) in _scaledLabels)
        {
            label.FontSize = fontSize * ratio;
        }

        ApplyRowSpacing(mainGrid, height, fontSize);
    }

    private static double GetCardVerticalThickness()
        => 2 * (CardPaddingVertical + CardStrokeThickness);

    /// <summary>
    /// When the width limits the font size, the text does not fill the height. Most of that
    /// remainder is spent on the gaps between the rows, the rest lands in the star row and
    /// therefore below the last block row.
    /// </summary>
    private void ApplyRowSpacing(Grid mainGrid, double height, double fontSize)
    {
        double neededHeight = fontSize * _requiredLineUnits * LineHeightRatio;
        double leftoverHeight = Math.Max(0, height - neededHeight);

        double distributedGap = leftoverHeight / _cardRowCount * RowSpacingShare;

        mainGrid.RowSpacing = (fontSize * MinRowGapRatio) + distributedGap;
    }
}
