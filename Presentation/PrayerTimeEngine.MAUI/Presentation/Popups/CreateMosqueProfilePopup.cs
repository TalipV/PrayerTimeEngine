using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Services;
using PrayerTimeEngine.Presentation.Localization;

namespace PrayerTimeEngine.Presentation.Popups;

/// <summary>The mosque the user wants a profile for.</summary>
public sealed record MosqueProfileRequest(EMosquePrayerTimeProviderType ProviderType, string ExternalID);

/// <summary>
/// Dialog for a new mosque profile. The mosque comes either from the Mawaqit app's "Share" button (offered
/// prominently on top, while this dialog is open) or from a browser link / identifier pasted into the text field.
/// The input is recognized while typing (<see cref="MosqueReferenceParser"/>), so the user sees which mosque
/// app and mosque will be used before the profile is created.
/// Returns the <see cref="MosqueProfileRequest"/>, or <c>null</c> when the dialog was cancelled.
/// </summary>
public sealed partial class CreateMosqueProfilePopup : Popup<MosqueProfileRequest?>
{
    private const double CORNER_RADIUS = 22;
    private const uint PULSE_HALF_PERIOD_MS = 800;

    private static readonly EMosquePrayerTimeProviderType[] s_providerTypes =
        [EMosquePrayerTimeProviderType.Mawaqit, EMosquePrayerTimeProviderType.MyMosq];

    private readonly Entry _inputEntry;
    private readonly Label _statusLabel;
    private readonly VerticalStackLayout _providerSelection;
    private readonly Dictionary<EMosquePrayerTimeProviderType, Button> _providerButtons = [];
    private readonly Button _createButton;

    private MosqueReference? _reference;
    private EMosquePrayerTimeProviderType _selectedProviderType = EMosquePrayerTimeProviderType.None;


    private bool _isPulsing;

    public CreateMosqueProfilePopup(bool isShareTargetSupported)
    {
        BackgroundColor = AppColors.Card;
        Padding = new Thickness(0);
        Margin = new Thickness(16, 24);

        _inputEntry = CreateInputEntry();
        _statusLabel = new Label
        {
            FontSize = 14,
            IsVisible = false,
            Margin = new Thickness(4, 8, 4, 0),
        };
        _providerSelection = CreateProviderSelection();
        _createButton = CreateCreateButton();

        var content = new VerticalStackLayout
        {
            Padding = new Thickness(20, 18, 20, 12),
            Spacing = 0,
            Children = { CreateHeader() }
        };

        if (isShareTargetSupported)
        {
            content.Children.Add(CreateShareCard());
            content.Children.Add(CreateOrSeparator());
        }

        content.Children.Add(CreateInputSection());
        content.Children.Add(_statusLabel);
        content.Children.Add(_providerSelection);
        content.Children.Add(_createButton);
        content.Children.Add(CreateCancelButton());

        double maximumHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density * 0.85;

        Content = new ScrollView
        {
            WidthRequest = 340,
            MaximumHeightRequest = maximumHeight,
            Content = content
        };

        UpdateRecognition();
    }

    /// <summary>
    /// Takes a text shared from another app (e.g. the Mawaqit app's share text) into the dialog.
    /// The recognized mosque is shown as its link in the text field (the mosque's name is looked up in the mosque app
    /// when the profile is created, so the language of the shared text does not matter).
    /// </summary>
    public void ApplySharedText(string sharedText)
    {
        MosqueReference? reference = MosqueReferenceParser.Parse(sharedText);

        string singleLineText = sharedText.ReplaceLineEndings(" ").Trim();

        _inputEntry.Text = reference is null
            ? singleLineText
            : reference.ProviderType switch
            {
                EMosquePrayerTimeProviderType.Mawaqit => $"https://mawaqit.net/de/{reference.ExternalID}",
                EMosquePrayerTimeProviderType.MyMosq => $"https://mymosq.com/mosque/{reference.ExternalID}",
                _ => singleLineText,
            };

        // TextChanged does not fire when the field already showed this link
        UpdateRecognition();
    }

    #region Header

    private View CreateHeader()
    {
        var closeButton = new Label
        {
            Text = "✕",
            FontSize = 18,
            TextColor = AppColors.TextMuted,
            Padding = new Thickness(12, 4, 0, 4),
            VerticalOptions = LayoutOptions.Center
        };
        closeButton.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await CloseAsync(null)) });

        var header = new Grid
        {
            Margin = new Thickness(0, 0, 0, 14),
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        header.Add(new Label
        {
            Text = AppResources.MosqueProfile_Title,
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = AppColors.Text,
            VerticalOptions = LayoutOptions.Center
        }, column: 0);
        header.Add(closeButton, column: 1);

        return header;
    }

    #endregion Header

    #region Share card

    /// <summary>
    /// The way via the Mawaqit app's "Share" button, as an accent-framed card with numbered steps and a pulsing
    /// "ready" indicator, so that it is noticed next to the text field and its time limit is clear.
    /// </summary>
    private Border CreateShareCard()
    {
        var icon = new Border
        {
            WidthRequest = 44,
            HeightRequest = 44,
            BackgroundColor = AppColors.Accent.WithAlpha(0.18f),
            StrokeThickness = 0,
            StrokeShape = new Ellipse(),
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = "📤",
                FontSize = 20,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
            },
        };

        var titleRow = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };
        titleRow.Add(icon, column: 0);
        titleRow.Add(new Label
        {
            Text = AppResources.MosqueProfile_ShareTitle,
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            TextColor = AppColors.Text,
            VerticalOptions = LayoutOptions.Center,
        }, column: 1);

        var steps = new VerticalStackLayout
        {
            Spacing = 8,
            Margin = new Thickness(0, 14, 0, 14),
            Children =
            {
                CreateStep(1, AppResources.MosqueProfile_ShareStep1),
                CreateStep(2, AppResources.MosqueProfile_ShareStep2),
                CreateStep(3, AppResources.MosqueProfile_ShareStep3),
            }
        };

        return new Border
        {
            BackgroundColor = AppColors.Accent.WithAlpha(0.07f),
            Stroke = AppColors.Accent,
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(16, 16, 16, 14),
            Content = new VerticalStackLayout
            {
                Children =
                {
                    titleRow,
                    steps,
                    CreateReadyIndicator(),
                }
            }
        };
    }

    private static Grid CreateStep(int number, string text)
    {
        var step = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        step.Add(new Border
        {
            WidthRequest = 24,
            HeightRequest = 24,
            BackgroundColor = AppColors.Accent,
            StrokeThickness = 0,
            StrokeShape = new Ellipse(),
            VerticalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = number.ToString(),
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = AppColors.Background,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
            },
        }, column: 0);

        step.Add(new Label
        {
            Text = text,
            FontSize = 15,
            TextColor = AppColors.Text,
            VerticalOptions = LayoutOptions.Center,
        }, column: 1);

        return step;
    }

    /// <summary>A pill with a pulsing dot: the app can receive a shared mosque right now, but only while the dialog is open.</summary>
    private Border CreateReadyIndicator()
    {
        var dot = new Ellipse
        {
            WidthRequest = 10,
            HeightRequest = 10,
            Fill = new SolidColorBrush(AppColors.Accent),
            VerticalOptions = LayoutOptions.Center,
        };

        // only while the dot is on screen: an animation of a view outside the visual tree ends at once
        dot.Loaded += (_, _) => StartPulsing(dot);
        dot.Unloaded += (_, _) => _isPulsing = false;

        return new Border
        {
            BackgroundColor = AppColors.Surface,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(12, 8),
            Content = new HorizontalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    dot,
                    new Label
                    {
                        Text = AppResources.MosqueProfile_ShareReady,
                        FontSize = 13,
                        TextColor = AppColors.TextMuted,
                        VerticalOptions = LayoutOptions.Center,
                        LineBreakMode = LineBreakMode.WordWrap,
                        MaximumWidthRequest = 240,
                    }
                }
            }
        };
    }

    private async void StartPulsing(View dot)
    {
        if (_isPulsing)
        {
            return;
        }

        _isPulsing = true;

        try
        {
            while (_isPulsing)
            {
                long startTickCount = Environment.TickCount64;

                await dot.FadeToAsync(0.25, PULSE_HALF_PERIOD_MS, Easing.SinInOut);
                await dot.FadeToAsync(1, PULSE_HALF_PERIOD_MS, Easing.SinInOut);

                // if the animations ended at once (e.g. animations are switched off on the device),
                // wait anyway instead of spinning on the main thread
                if (Environment.TickCount64 - startTickCount < PULSE_HALF_PERIOD_MS)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(PULSE_HALF_PERIOD_MS * 2));
                }
            }
        }
        catch (Exception)
        {
            // only decoration: a failed animation (e.g. after the popup's handler is gone) must not crash the app
        }
    }

    private static Grid CreateOrSeparator()
    {
        var separator = new Grid
        {
            Margin = new Thickness(0, 18),
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };
        separator.Add(new BoxView { HeightRequest = 1, Color = AppColors.Border, VerticalOptions = LayoutOptions.Center }, column: 0);
        separator.Add(new Label
        {
            Text = AppResources.MosqueProfile_Or,
            FontSize = 13,
            TextColor = AppColors.TextMuted,
        }, column: 1);
        separator.Add(new BoxView { HeightRequest = 1, Color = AppColors.Border, VerticalOptions = LayoutOptions.Center }, column: 2);

        return separator;
    }

    #endregion Share card

    #region Input

    private Entry CreateInputEntry()
    {
        var entry = new Entry
        {
            Placeholder = AppResources.MosqueProfile_InputPlaceholder,
            BackgroundColor = Colors.Transparent,
            TextColor = AppColors.Text,
            PlaceholderColor = AppColors.TextMuted,
            FontSize = 15,
            Keyboard = Keyboard.Url,
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing,
            VerticalOptions = LayoutOptions.Center,
        };

        entry.TextChanged += (_, _) => UpdateRecognition();

#if ANDROID
        // the field sits in a rounded border, the native underline of the EditText would only add noise
        entry.HandlerChanged += (_, _) =>
        {
            if (entry.Handler?.PlatformView is Android.Widget.EditText editText)
            {
                editText.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
            }
        };
#endif

        return entry;
    }

    private VerticalStackLayout CreateInputSection()
    {
        var pasteButton = new Button
        {
            Text = AppResources.MosqueProfile_Paste,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = AppColors.Accent,
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            Padding = new Thickness(10, 0),
            HeightRequest = 40,
            VerticalOptions = LayoutOptions.Center,
            Command = new Command(async () => await PasteFromClipboard()),
        };

        var inputRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        inputRow.Add(_inputEntry, column: 0);
        inputRow.Add(pasteButton, column: 1);

        return new VerticalStackLayout
        {
            Children =
            {
                new Label
                {
                    Text = AppResources.MosqueProfile_InputTitle,
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = AppColors.Text,
                    Margin = new Thickness(4, 0, 4, 8),
                },
                new Border
                {
                    BackgroundColor = AppColors.Surface,
                    Stroke = AppColors.Border,
                    StrokeThickness = 1,
                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                    Padding = new Thickness(10, 2, 2, 2),
                    Content = inputRow,
                },
                new Label
                {
                    Text = AppResources.MosqueProfile_InputHint,
                    FontSize = 13,
                    TextColor = AppColors.TextMuted,
                    Margin = new Thickness(4, 6, 4, 0),
                },
            }
        };
    }

    private async Task PasteFromClipboard()
    {
        try
        {
            string? clipboardText = await Clipboard.Default.GetTextAsync();

            if (!string.IsNullOrWhiteSpace(clipboardText))
            {
                // pasted like a shared text, so that the name in a copied Mawaqit share text is kept as well
                ApplySharedText(clipboardText);
            }
        }
        catch (Exception)
        {
            // the clipboard can be unavailable; the user can still paste into the field by hand
        }
    }

    #endregion Input

    #region Recognition

    private VerticalStackLayout CreateProviderSelection()
    {
        var buttons = new HorizontalStackLayout { Spacing = 8 };

        foreach (EMosquePrayerTimeProviderType providerType in s_providerTypes)
        {
            var button = new Button
            {
                Text = providerType.ToString(),
                FontSize = 14,
                CornerRadius = 18,
                HeightRequest = 38,
                Padding = new Thickness(16, 0),
                BorderWidth = 1,
                Command = new Command(() =>
                {
                    _selectedProviderType = providerType;
                    UpdateRecognition();
                }),
            };

            _providerButtons[providerType] = button;
            buttons.Children.Add(button);
        }

        return new VerticalStackLayout
        {
            IsVisible = false,
            Spacing = 8,
            Margin = new Thickness(4, 10, 4, 0),
            Children =
            {
                new Label
                {
                    Text = AppResources.MosqueProfile_SelectApp,
                    FontSize = 13,
                    TextColor = AppColors.TextMuted,
                },
                buttons
            }
        };
    }

    /// <summary>Recognizes the mosque in the text field and shows the result, the app choice and the state of the create button.</summary>
    private void UpdateRecognition()
    {
        string input = _inputEntry.Text ?? "";
        MosqueReference? reference = MosqueReferenceParser.Parse(input);
        _reference = reference;

        bool needsProviderSelection = reference is not null && reference.ProviderType == EMosquePrayerTimeProviderType.None;
        _providerSelection.IsVisible = needsProviderSelection;

        foreach ((EMosquePrayerTimeProviderType providerType, Button button) in _providerButtons)
        {
            bool isSelected = providerType == _selectedProviderType;
            button.BackgroundColor = isSelected ? AppColors.Accent : AppColors.Surface;
            button.TextColor = isSelected ? AppColors.Background : AppColors.Text;
            button.BorderColor = isSelected ? AppColors.Accent : AppColors.Border;
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            _statusLabel.IsVisible = false;
        }
        else if (reference is null)
        {
            _statusLabel.IsVisible = true;
            _statusLabel.Text = AppResources.MosqueProfile_NotRecognized;
            _statusLabel.TextColor = AppColors.CurrentTime;
        }
        else if (!needsProviderSelection)
        {
            _statusLabel.IsVisible = true;
            _statusLabel.Text = "✓ " + string.Format(
                AppResources.MosqueProfile_Recognized,
                reference.ProviderType,
                reference.ExternalID);
            _statusLabel.TextColor = AppColors.Accent;
        }
        else
        {
            _statusLabel.IsVisible = false;
        }

        bool canCreate = GetRequest() is not null;
        _createButton.IsEnabled = canCreate;
        _createButton.Opacity = canCreate ? 1 : 0.4;
    }

    private MosqueProfileRequest? GetRequest()
    {
        if (_reference is null)
        {
            return null;
        }

        EMosquePrayerTimeProviderType providerType = _reference.ProviderType != EMosquePrayerTimeProviderType.None
            ? _reference.ProviderType
            : _selectedProviderType;

        return providerType == EMosquePrayerTimeProviderType.None
            ? null
            : new MosqueProfileRequest(providerType, _reference.ExternalID);
    }

    #endregion Recognition

    #region Buttons

    private Button CreateCreateButton()
    {
        return new Button
        {
            Text = AppResources.MosqueProfile_Create,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = AppColors.Background,
            BackgroundColor = AppColors.Accent,
            CornerRadius = 14,
            HeightRequest = 50,
            Margin = new Thickness(0, 20, 0, 0),
            Command = new Command(async () =>
            {
                MosqueProfileRequest? request = GetRequest();

                if (request is not null)
                {
                    await CloseAsync(request);
                }
            }),
        };
    }

    private Button CreateCancelButton()
    {
        return new Button
        {
            Text = AppResources.Common_Cancel,
            FontSize = 15,
            TextColor = AppColors.TextMuted,
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            HeightRequest = 46,
            Margin = new Thickness(0, 4, 0, 0),
            Command = new Command(async () => await CloseAsync(null)),
        };
    }

    #endregion Buttons

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
