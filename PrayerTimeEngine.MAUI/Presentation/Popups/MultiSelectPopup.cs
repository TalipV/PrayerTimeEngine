using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace PrayerTimeEngine.Presentation.Popups;

public record MultiSelectOption(string Text = "", bool IsSelected = false);

public sealed partial class MultiSelectPopup : Popup<List<MultiSelectOption>>
{
    public string Title { get; }
    public ObservableCollection<MultiSelectOption> Options { get; }

    public MultiSelectPopup(string title, ICollection<MultiSelectOption> options)
    {
        Title = title;
        Options = new ObservableCollection<MultiSelectOption>(options);
        BindingContext = this;

        Margin = new Thickness(left: 20, top: 20, right: 20, bottom: 0);
        Padding = new Thickness(left: 5, top: 0, right: 5, bottom: 0);

        Content = BuildContent();
    }

    VerticalStackLayout BuildContent() =>
        new()
        {
            HorizontalOptions = LayoutOptions.Center,
            MaximumWidthRequest = 260,
            Children =
            {
                new Label()
                    .Text(Title)
                    .Font(bold: true)
                    .FontSize(16)
                    .TextColor(Colors.Black),

                new ScrollView
                {
                    HeightRequest = 250,
                    Content = new CollectionView
                    {
                        ItemsSource = Options,
                        ItemTemplate = new DataTemplate(() =>
                        {
                            var grid = new Grid
                            {
                                ColumnDefinitions = Columns.Define(Auto, Star),
                                Padding = new Thickness(0, 4)
                            };

                            var checkBox = new CheckBox()
                                .Bind(CheckBox.IsCheckedProperty, nameof(MultiSelectOption.IsSelected))
                                .CenterVertical()
                                .Column(0);

                            var label = new Label()
                                .Bind(Label.TextProperty, nameof(MultiSelectOption.Text))
                                .TextColor(Colors.Black)
                                .LineBreakMode(LineBreakMode.WordWrap)
                                .CenterVertical()
                                .Column(1);

                            grid.Children.Add(checkBox);
                            grid.Children.Add(label);

                            return grid;
                        })
                    }
                },

                new HorizontalStackLayout
                {
                    Children =
                    {
                        new Button()
                            .Text("OK")
                            .FontSize(14)
                            .Invoke(b => b.Clicked += OnOkClicked),

                        new Button()
                            .Text("Cancel")
                            .FontSize(14)
                            .Invoke(b => b.Clicked += OnCancelClicked),
                    }
                }
                .Paddings(10, 0, 0, 0)
                .CenterHorizontal()
            }
        };

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync([]);
    }

    async void OnOkClicked(object sender, EventArgs e)
    {
        List<MultiSelectOption> selected = Options.Where(o => o.IsSelected).ToList();
        await CloseAsync(selected);
    }
}

