using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Shapes;
using PrayerTimeEngine.Presentation.Localization;
using PrayerTimeEngine.Services.Logging;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace PrayerTimeEngine.Presentation.Pages.LogViewer;

/// <summary>
/// The log entries of the running app: filterable by level (several at once) and by text, with the app wide
/// level from which on entries are written at all, and with sharing of the log files or the shown entries.
/// </summary>
public class LogsPage : ContentPage
{
    public LogsPage(LogsPageViewModel viewModel)
    {
        BindingContext = viewModel;
        Title = AppResources.Logs_Title;
        BackgroundColor = AppColors.Background;

        var contentGrid = new Grid
        {
            RowDefinitions = Rows.Define(
                GridLength.Auto,
                GridLength.Auto,
                GridLength.Auto,
                GridLength.Auto,
                GridLength.Auto,
                GridLength.Star),
            RowSpacing = 8,
            Padding = new Thickness(10),
        };

        contentGrid.Add(CreateSearchField(), column: 0, row: 0);
        contentGrid.Add(CreateLevelFilter(), column: 0, row: 1);
        contentGrid.Add(CreateMinimumLevelRow(), column: 0, row: 2);
        contentGrid.Add(CreateActionButtons(), column: 0, row: 3);
        contentGrid.Add(
            new Label()
                .FontSize(12)
                .TextColor(AppColors.TextMuted)
                .Bind(Label.TextProperty, nameof(LogsPageViewModel.StatusText)),
            column: 0, row: 4);
        contentGrid.Add(CreateEntryList(), column: 0, row: 5);

        Content = contentGrid;
    }

    private static Border CreateSearchField()
    {
        Entry searchEntry = new Entry
        {
            Placeholder = AppResources.Logs_SearchPlaceholder,
            TextColor = AppColors.Text,
            PlaceholderColor = AppColors.TextMuted,
            BackgroundColor = Colors.Transparent,
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing,
        }
        .Bind(Entry.TextProperty, nameof(LogsPageViewModel.SearchText), mode: BindingMode.TwoWay);

        return new Border
        {
            StrokeThickness = 1,
            Stroke = AppColors.Border,
            BackgroundColor = AppColors.Card,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = new Thickness(8, 0),
            Content = searchEntry,
        };
    }

    /// <summary>One chip per level; tapping it shows or hides the entries of that level.</summary>
    private FlexLayout CreateLevelFilter()
    {
        var levelFilter = new FlexLayout
        {
            Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
            JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start,
        };

        BindableLayout.SetItemTemplate(levelFilter, new DataTemplate(() =>
        {
            Label chipLabel = new Label()
                .FontSize(13)
                .Bind(Label.TextProperty, nameof(LogLevelFilterOption.DisplayText))
                .Bind<Label, bool, Color>(
                    Label.TextColorProperty,
                    nameof(LogLevelFilterOption.IsSelected),
                    convert: isSelected => isSelected ? AppColors.Surface : AppColors.TextMuted);

            Border chip = new Border
            {
                StrokeThickness = 1,
                Stroke = AppColors.Border,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Padding = new Thickness(12, 6),
                Margin = new Thickness(0, 0, 6, 6),
                Content = chipLabel,
            }
            .Bind<Border, bool, Color>(
                BackgroundColorProperty,
                nameof(LogLevelFilterOption.IsSelected),
                convert: isSelected => isSelected ? AppColors.Accent : AppColors.Card);

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (sender, _) =>
            {
                if (sender is BindableObject { BindingContext: LogLevelFilterOption option })
                {
                    option.IsSelected = !option.IsSelected;
                }
            };
            chip.GestureRecognizers.Add(tapGestureRecognizer);

            return chip;
        }));
        levelFilter.Bind(BindableLayout.ItemsSourceProperty, nameof(LogsPageViewModel.LevelFilterOptions));

        return levelFilter;
    }

    private static Grid CreateMinimumLevelRow()
    {
        var minimumLevelRow = new Grid
        {
            ColumnDefinitions = Columns.Define(GridLength.Auto, GridLength.Star),
            ColumnSpacing = 8,
        };

        minimumLevelRow.Add(
            new Label
            {
                Text = AppResources.Logs_MinimumLevel_Label,
                TextColor = AppColors.Text,
                VerticalTextAlignment = TextAlignment.Center,
            },
            column: 0, row: 0);

        Picker minimumLevelPicker = new Picker
        {
            Title = AppResources.Logs_MinimumLevel_Label,
            TextColor = AppColors.Text,
            TitleColor = AppColors.TextMuted,
        }
        .Bind(Picker.ItemsSourceProperty, nameof(LogsPageViewModel.MinimumLevelOptions))
        .Bind(Picker.SelectedItemProperty, nameof(LogsPageViewModel.SelectedMinimumLevelOption), mode: BindingMode.TwoWay);
        minimumLevelRow.Add(minimumLevelPicker, column: 1, row: 0);

        return minimumLevelRow;
    }

    private FlexLayout CreateActionButtons()
    {
        static Button CreateButton(string text, string commandPath) =>
            new Button
            {
                Text = text,
                FontSize = 13,
                TextColor = AppColors.Text,
                BackgroundColor = AppColors.Surface,
                CornerRadius = 10,
                Padding = new Thickness(12, 6),
                Margin = new Thickness(0, 0, 6, 6),
                HeightRequest = 38,
            }
            .Bind(Button.CommandProperty, commandPath);

        return new FlexLayout
        {
            Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
            Children =
            {
                CreateButton(AppResources.Logs_ShareLogFiles, nameof(LogsPageViewModel.ShareLogFilesCommand)),
                CreateButton(AppResources.Logs_ShareFilteredEntries, nameof(LogsPageViewModel.ShareFilteredEntriesCommand)),
                CreateButton(AppResources.Logs_Reload, nameof(LogsPageViewModel.ReloadCommand)),
                CreateButton(AppResources.Logs_AllLevels, nameof(LogsPageViewModel.SelectAllLevelsCommand)),
                CreateButton(AppResources.Logs_Clear, nameof(LogsPageViewModel.ClearCommand)),
            }
        };
    }

    private CollectionView CreateEntryList()
    {
        return new CollectionView
        {
            SelectionMode = SelectionMode.None,
            ItemTemplate = new DataTemplate(CreateEntryView),
            EmptyView = new Label
            {
                Text = AppResources.Logs_NoEntries,
                TextColor = AppColors.TextMuted,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 20),
            },
        }
        .Bind(ItemsView.ItemsSourceProperty, nameof(LogsPageViewModel.FilteredEntries));
    }

    /// <summary>
    /// One entry: time, level and source in the colour of the level, below it the beginning of the message.
    /// Tapping it shows the complete entry (with the exception) and offers to copy it.
    /// </summary>
    private View CreateEntryView()
    {
        Label headerLabel = new Label()
            .FontSize(11)
            .Font(bold: true)
            .Bind<Label, LogEntry, string>(
                Label.TextProperty,
                Binding.SelfPath,
                convert: entry => entry is null ? string.Empty : $"{entry.Timestamp:HH:mm:ss.fff}  {LogsPageViewModel.GetLevelName(entry.Level)}  {GetShortCategory(entry.Category)}")
            .Bind<Label, LogEntry, Color>(
                Label.TextColorProperty,
                Binding.SelfPath,
                convert: entry => GetLevelColor(entry?.Level ?? LogLevel.None));
        headerLabel.LineBreakMode = LineBreakMode.TailTruncation;

        Label messageLabel = new Label()
            .FontSize(13)
            .TextColor(AppColors.Text)
            .Bind<Label, LogEntry, string>(
                Label.TextProperty,
                Binding.SelfPath,
                convert: entry => entry?.Exception is null ? entry?.Message : $"{entry.Message}{System.Environment.NewLine}⚠ {entry.Exception.GetType().Name}: {entry.Exception.Message}");
        messageLabel.MaxLines = 6;
        messageLabel.LineBreakMode = LineBreakMode.TailTruncation;

        var entryView = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = AppColors.Card,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(8, 6),
            Margin = new Thickness(0, 0, 0, 6),
            Content = new VerticalStackLayout
            {
                Spacing = 2,
                Children = { headerLabel, messageLabel }
            },
        };

        var tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += async (sender, _) =>
        {
            if (sender is BindableObject { BindingContext: LogEntry entry })
            {
                await ShowEntryDetails(entry);
            }
        };
        entryView.GestureRecognizers.Add(tapGestureRecognizer);

        return entryView;
    }

    private async Task ShowEntryDetails(LogEntry entry)
    {
        bool copy = await DisplayAlertAsync(
            LogsPageViewModel.GetLevelName(entry.Level),
            entry.FullText,
            AppResources.Logs_Copy,
            AppResources.Common_Ok);

        if (copy)
        {
            await Clipboard.Default.SetTextAsync(entry.FullText);
        }
    }

    /// <summary>"PrayerTimeEngine.Core.Domain.Foo.BarManager" becomes "BarManager".</summary>
    private static string GetShortCategory(string category)
    {
        int lastDotIndex = category.LastIndexOf('.');
        return lastDotIndex < 0 ? category : category[(lastDotIndex + 1)..];
    }

    private static Color GetLevelColor(LogLevel level) => level switch
    {
        LogLevel.Trace or LogLevel.Debug => AppColors.TextMuted,
        LogLevel.Warning => AppColors.Accent,
        LogLevel.Error or LogLevel.Critical => AppColors.CurrentTime,
        _ => AppColors.Text,
    };
}
