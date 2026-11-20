using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using PrayerTimeEngine.Presentation.Localization;

namespace PrayerTimeEngine.Presentation.Popups;

/// <summary>
/// Shown on the first start: the app is only offered for private use and only to people with an active
/// Fazilet subscription. Returns <c>true</c> once the user confirmed it with the button; <c>false</c> when the
/// popup was closed in another way (e.g. the back button), so that it is shown again on the next start.
/// </summary>
public sealed partial class WelcomePopup : Popup<bool>
{
    private const double CORNER_RADIUS = 22;

    public WelcomePopup()
    {
        BackgroundColor = AppColors.Card;
        Padding = new Thickness(0);
        Margin = new Thickness(24);

        Content = new VerticalStackLayout
        {
            WidthRequest = 330,
            Padding = new Thickness(24, 28, 24, 20),
            Spacing = 0,
            Children =
            {
                CreateIcon(),
                new Label
                {
                    Text = AppResources.Welcome_Title,
                    FontSize = 21,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = AppColors.Text,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 18, 0, 6),
                },
                new Label
                {
                    Text = AppResources.Welcome_Intro,
                    FontSize = 15,
                    TextColor = AppColors.TextMuted,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 18),
                },
                CreateNoticeCard(),
                CreateAcceptButton(),
            }
        };
    }

    /// <summary>A greeting glyph in an accent-tinted circle.</summary>
    private static Border CreateIcon()
    {
        return new Border
        {
            WidthRequest = 64,
            HeightRequest = 64,
            HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = AppColors.Accent.WithAlpha(0.15f),
            StrokeThickness = 0,
            StrokeShape = new Ellipse(),
            Content = new Label
            {
                Text = "🕌",
                FontSize = 30,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
            },
        };
    }

    /// <summary>The two conditions, framed in the accent color so that they are not overlooked.</summary>
    private static Border CreateNoticeCard()
    {
        return new Border
        {
            BackgroundColor = AppColors.Accent.WithAlpha(0.07f),
            Stroke = AppColors.Accent,
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(16),
            Content = new VerticalStackLayout
            {
                Spacing = 16,
                Children =
                {
                    CreateNotice("🔒", AppResources.Welcome_PrivateTitle, AppResources.Welcome_PrivateText),
                    CreateNotice("✅", AppResources.Welcome_FaziletTitle, AppResources.Welcome_FaziletText),
                }
            }
        };
    }

    private static Grid CreateNotice(string icon, string title, string text)
    {
        var notice = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        notice.Add(new Label
        {
            Text = icon,
            FontSize = 20,
            VerticalOptions = LayoutOptions.Start,
        }, column: 0);

        notice.Add(new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                new Label
                {
                    Text = title,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = AppColors.Text,
                },
                new Label
                {
                    Text = text,
                    FontSize = 14,
                    LineHeight = 1.2,
                    TextColor = AppColors.TextMuted,
                },
            }
        }, column: 1);

        return notice;
    }

    private Button CreateAcceptButton()
    {
        return new Button
        {
            Text = AppResources.Welcome_Accept,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = AppColors.Background,
            BackgroundColor = AppColors.Accent,
            CornerRadius = 14,
            HeightRequest = 50,
            Margin = new Thickness(0, 22, 0, 0),
            Command = new Command(async () => await CloseAsync(true)),
        };
    }

    /// <summary>Overlay and shape of the popup, to be passed to <c>ShowPopupAsync</c>.</summary>
    public static CommunityToolkit.Maui.PopupOptions CreatePopupOptions()
    {
        return new CommunityToolkit.Maui.PopupOptions
        {
            // the notice has to be confirmed with the button, not just tapped away
            CanBeDismissedByTappingOutsideOfPopup = false,
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
