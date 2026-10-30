using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;

namespace PrayerTimeEngine.Presentation.Popups;

/// <summary>
/// One entry of the options menu. <paramref name="ID"/> identifies the entry independent of its
/// (translated) <paramref name="Text"/>; <paramref name="IsDestructive"/> marks entries that delete or reset data.
/// </summary>
public record OptionsMenuItem(string ID, string Text, bool IsDestructive = false);

/// <summary>A titled group of entries in the options menu.</summary>
public record OptionsMenuSection(string Title, IReadOnlyList<OptionsMenuItem> Items);

/// <summary>
/// The options menu of the main page in the app palette: all entries on one sheet, grouped into
/// sections, instead of the native action sheets that needed a sub menu (and a way back) per group.
/// Returns the <see cref="OptionsMenuItem.ID"/> of the tapped entry, or <c>null</c> when the menu was closed.
/// </summary>
public sealed partial class OptionsMenuPopup : Popup<string?>
{
    private const double CORNER_RADIUS = 18;

    public OptionsMenuPopup(string title, string? subtitle, IReadOnlyList<OptionsMenuSection> sections)
    {
        BackgroundColor = AppColors.Card;
        Padding = new Thickness(0);
        Margin = new Thickness(16, 24);

        double maximumHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density * 0.8;

        Content = new VerticalStackLayout
        {
            WidthRequest = 340,
            Children =
            {
                CreateHeader(title, subtitle),
                new ScrollView
                {
                    MaximumHeightRequest = maximumHeight - 80,
                    Content = CreateSections(sections)
                }
            }
        };
    }

    private View CreateHeader(string title, string? subtitle)
    {
        var titleLabel = new Label
        {
            Text = title,
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = AppColors.Text
        };

        var closeButton = new Label
        {
            Text = "✕",
            FontSize = 18,
            TextColor = AppColors.TextMuted,
            Padding = new Thickness(12, 4),
            VerticalOptions = LayoutOptions.Center
        };
        closeButton.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await CloseAsync(null)) });

        var titleStack = new VerticalStackLayout { Children = { titleLabel } };

        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            titleStack.Children.Add(new Label
            {
                Text = subtitle,
                FontSize = 13,
                TextColor = AppColors.TextMuted
            });
        }

        var header = new Grid
        {
            Padding = new Thickness(20, 18, 8, 10),
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        header.Add(titleStack, column: 0);
        header.Add(closeButton, column: 1);

        return header;
    }

    private VerticalStackLayout CreateSections(IReadOnlyList<OptionsMenuSection> sections)
    {
        var sectionsLayout = new VerticalStackLayout { Padding = new Thickness(12, 0, 12, 14), Spacing = 14 };

        foreach (OptionsMenuSection section in sections)
        {
            sectionsLayout.Children.Add(new Label
            {
                Text = section.Title,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = AppColors.TextMuted,
                Margin = new Thickness(8, 0, 0, -8)
            });

            var itemsLayout = new VerticalStackLayout();

            for (int index = 0; index < section.Items.Count; index++)
            {
                if (index > 0)
                {
                    itemsLayout.Children.Add(new BoxView
                    {
                        HeightRequest = 1,
                        Color = AppColors.Card,
                        Margin = new Thickness(14, 0)
                    });
                }

                itemsLayout.Children.Add(CreateItem(section.Items[index]));
            }

            sectionsLayout.Children.Add(new Border
            {
                BackgroundColor = AppColors.Surface,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = itemsLayout
            });
        }

        return sectionsLayout;
    }

    private View CreateItem(OptionsMenuItem item)
    {
        var textLabel = new Label
        {
            Text = item.Text,
            FontSize = 15,
            TextColor = item.IsDestructive ? AppColors.CurrentTime : AppColors.Text,
            VerticalOptions = LayoutOptions.Center
        };

        var chevronLabel = new Label
        {
            Text = "›",
            FontSize = 20,
            TextColor = AppColors.TextMuted,
            VerticalOptions = LayoutOptions.Center
        };

        // a background is needed for the whole row to receive the tap on Android, not only the text
        var row = new Grid
        {
            BackgroundColor = AppColors.Surface,
            Padding = new Thickness(14, 12),
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        row.Add(textLabel, column: 0);
        row.Add(chevronLabel, column: 1);

        row.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await CloseAsync(item.ID)) });

        return row;
    }

    /// <summary>Overlay and shape of the popup, to be passed to <c>ShowPopupAsync</c>.</summary>
    public static CommunityToolkit.Maui.PopupOptions CreatePopupOptions()
    {
        return new CommunityToolkit.Maui.PopupOptions
        {
            PageOverlayColor = Color.FromRgba(0, 0, 0, 0.6),
            Shape = new RoundRectangle
            {
                CornerRadius = CORNER_RADIUS,
                Stroke = AppColors.Border,
                StrokeThickness = 1
            }
        };
    }
}
