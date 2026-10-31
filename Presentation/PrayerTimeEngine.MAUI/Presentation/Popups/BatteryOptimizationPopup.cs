using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using PrayerTimeEngine.Presentation.Localization;

namespace PrayerTimeEngine.Presentation.Popups;

/// <summary>
/// Short hint in the app palette that the prayer time notification needs an exemption from the
/// battery optimization: an icon, one sentence and one clear button, instead of a native alert
/// with a text block about system settings.
/// Returns <c>true</c> when the user wants to allow it now, <c>false</c> for "later" or when the popup was dismissed.
/// </summary>
public sealed partial class BatteryOptimizationPopup : Popup<bool>
{
    private const double CORNER_RADIUS = 22;

    public BatteryOptimizationPopup()
    {
        BackgroundColor = AppColors.Card;
        Padding = new Thickness(0);
        Margin = new Thickness(24);

        Content = new VerticalStackLayout
        {
            WidthRequest = 320,
            Padding = new Thickness(24, 28, 24, 16),
            Spacing = 0,
            Children =
            {
                CreateIcon(),
                new Label
                {
                    Text = AppResources.BatteryOptimization_Title,
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = AppColors.Text,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 18, 0, 8),
                },
                new Label
                {
                    Text = AppResources.BatteryOptimization_Message,
                    FontSize = 15,
                    LineHeight = 1.25,
                    TextColor = AppColors.TextMuted,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 24),
                },
                CreateAcceptButton(),
                CreateDeclineButton(),
            }
        };
    }

    /// <summary>A battery glyph in an accent-tinted circle.</summary>
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
                Text = "🔋",
                FontSize = 30,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
            },
        };
    }

    private Button CreateAcceptButton()
    {
        return new Button
        {
            Text = AppResources.BatteryOptimization_Accept,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = AppColors.Background,
            BackgroundColor = AppColors.Accent,
            CornerRadius = 14,
            HeightRequest = 50,
            Command = new Command(async () => await CloseAsync(true)),
        };
    }

    private Button CreateDeclineButton()
    {
        return new Button
        {
            Text = AppResources.BatteryOptimization_Decline,
            FontSize = 15,
            TextColor = AppColors.TextMuted,
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            HeightRequest = 46,
            Margin = new Thickness(0, 6, 0, 0),
            Command = new Command(async () => await CloseAsync(false)),
        };
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
