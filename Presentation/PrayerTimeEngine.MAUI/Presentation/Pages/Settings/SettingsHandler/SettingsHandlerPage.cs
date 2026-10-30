using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Presentation.Pages.Settings.SettingsContent;
using PrayerTimeEngine.Presentation.Services;
using PrayerTimeEngine.Presentation.Localization;

namespace PrayerTimeEngine.Presentation.Pages.Settings.SettingsHandler;

/// <summary>
/// Settings of one section of the day (e.g. Fajr) with one tab per configurable time.
/// <para>
/// The section is the title of the page, so the tabs only name the time within it ("Start" instead
/// of "FajrStart"). The tabs are pills in a horizontally scrollable row, which also fits sections with
/// many times, and the content can be swiped to the neighbouring tab.
/// </para>
/// </summary>
public partial class SettingsHandlerPage : ContentPage
{
    private const uint SWIPE_ANIMATION_LENGTH = 140;
    private const double SWIPE_ANIMATION_OFFSET = 40;

    private readonly SettingsHandlerPageViewModel _viewModel;
    private readonly ToastMessageService _toastMessageService;
    private readonly ILogger<SettingsHandlerPage> _logger;

    private readonly List<ContentView> _tabContents = [];
    private readonly List<Border> _tabHeaders = [];
    private readonly HorizontalStackLayout _tabHeaderLayout;
    private readonly ScrollView _tabHeaderScrollView;
    private readonly ContentView _tabContentHost;
    private readonly Label _titleLabel;
    private readonly Label _subtitleLabel;

    private int _selectedTabIndex = -1;

    public SettingsHandlerPage(
            SettingsHandlerPageViewModel viewModel,
            ToastMessageService toastMessageService,
            ILogger<SettingsHandlerPage> logger
        )
    {
        _toastMessageService = toastMessageService;
        _logger = logger;

        _titleLabel = new Label
        {
            FontAttributes = FontAttributes.Bold,
            FontSize = 22,
            TextColor = AppColors.Text,
            VerticalTextAlignment = TextAlignment.Center
        };
        _subtitleLabel = new Label
        {
            FontSize = 13,
            TextColor = AppColors.TextMuted
        };

        NavigationPage.SetTitleView(this, new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Children = { _titleLabel, _subtitleLabel }
        });

        BackgroundColor = AppColors.Background;

        _tabHeaderLayout = new HorizontalStackLayout { Spacing = 22, Padding = new Thickness(18, 0) };
        _tabHeaderScrollView = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
            Content = _tabHeaderLayout
        };

        _tabContentHost = new ContentView();

        // the transparent background makes the whole area receive the swipe, not only the controls on it
        var swipeArea = new Grid
        {
            BackgroundColor = Colors.Transparent,
            Children = { _tabContentHost }
        };
        swipeArea.GestureRecognizers.Add(CreateSwipeGestureRecognizer(SwipeDirection.Left, step: +1));
        swipeArea.GestureRecognizers.Add(CreateSwipeGestureRecognizer(SwipeDirection.Right, step: -1));

        Content = new Grid
        {
            Padding = new Thickness(0, 12, 0, 0),
            RowSpacing = 4,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            },
            Children =
            {
                _tabHeaderScrollView,
                swipeArea.Row(1)
            }
        };

        BindingContext = _viewModel = viewModel;

        viewModel.Initialized += OnViewModelInitialized;
    }

    private void OnViewModelInitialized()
    {
        _titleLabel.Text = LocalizedNames.GetSectionName(_viewModel.Section);
        _subtitleLabel.Text = _viewModel.ProfileName;

        for (int index = 0; index < _viewModel.SettingsContentPages.Count; index++)
        {
            SettingsContentPage settingsContentPage = _viewModel.SettingsContentPages[index];

            View content = settingsContentPage.Content;
            settingsContentPage.RemoveLogicalChild(content); // Causes warning if View is added to another page if not removed from former page

            _tabContents.Add(new ContentView
            {
                Content = content,
                BindingContext = settingsContentPage.ViewModel
            });

            // the section is already the page title, so a tab only names the time within it
            Border tabHeader = CreateTabHeader(LocalizedNames.GetTimeTypeName(settingsContentPage.ViewModel.TimeType), index);
            _tabHeaders.Add(tabHeader);
            _tabHeaderLayout.Children.Add(tabHeader);

            // added so late because moving around the contents triggers warning level binding errors
            settingsContentPage.AddDataBindings();
        }

        SelectTab(0, direction: 0);
    }

    private Border CreateTabHeader(string title, int index)
    {
        // plain text tabs with an underline under the selected one, no filled pills
        var tabHeader = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Colors.Transparent,
            Padding = new Thickness(4, 8, 4, 0),
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Label
                    {
                        Text = title,
                        FontSize = 16,
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    new BoxView
                    {
                        HeightRequest = 2,
                        Color = AppColors.Text
                    }
                }
            }
        };

        tabHeader.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() => SelectTab(index, direction: Math.Sign(index - _selectedTabIndex)))
        });

        return tabHeader;
    }

    private SwipeGestureRecognizer CreateSwipeGestureRecognizer(SwipeDirection swipeDirection, int step)
    {
        var swipeGestureRecognizer = new SwipeGestureRecognizer { Direction = swipeDirection };
        swipeGestureRecognizer.Swiped += (_, _) => SelectTab(_selectedTabIndex + step, direction: step);
        return swipeGestureRecognizer;
    }

    /// <summary>
    /// Shows the tab at <paramref name="index"/>. <paramref name="direction"/> is the side the new
    /// content comes from (+1 from the right, -1 from the left, 0 without animation).
    /// </summary>
    private async void SelectTab(int index, int direction)
    {
        if (index < 0 || index >= _tabContents.Count || index == _selectedTabIndex)
        {
            return;
        }

        _selectedTabIndex = index;

        for (int headerIndex = 0; headerIndex < _tabHeaders.Count; headerIndex++)
        {
            bool isSelected = headerIndex == index;
            Border tabHeader = _tabHeaders[headerIndex];

            var headerLayout = (VerticalStackLayout)tabHeader.Content;
            var titleLabel = (Label)headerLayout.Children[0];
            var underline = (BoxView)headerLayout.Children[1];

            titleLabel.TextColor = isSelected ? AppColors.Text : AppColors.TextMuted;
            titleLabel.FontAttributes = isSelected ? FontAttributes.Bold : FontAttributes.None;
            underline.Opacity = isSelected ? 1 : 0;
        }

        try
        {
            if (direction != 0)
            {
                // short slide in the swipe direction, so the change of the content is noticeable
                await Task.WhenAll(
                    _tabContentHost.TranslateToAsync(-direction * SWIPE_ANIMATION_OFFSET, 0, SWIPE_ANIMATION_LENGTH / 2, Easing.CubicIn),
                    _tabContentHost.FadeToAsync(0, SWIPE_ANIMATION_LENGTH / 2));
            }

            _tabContentHost.Content = _tabContents[index];

            if (direction != 0)
            {
                _tabContentHost.TranslationX = direction * SWIPE_ANIMATION_OFFSET;
                await Task.WhenAll(
                    _tabContentHost.TranslateToAsync(0, 0, SWIPE_ANIMATION_LENGTH / 2, Easing.CubicOut),
                    _tabContentHost.FadeToAsync(1, SWIPE_ANIMATION_LENGTH / 2));
            }

            await _tabHeaderScrollView.ScrollToAsync(_tabHeaders[index], ScrollToPosition.MakeVisible, animated: true);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while switching the settings tab");
        }
    }

    protected override async void OnDisappearing()
    {
        try
        {
            base.OnDisappearing();

            // for ViewModel to handle setting saving
            await OnDisappearingForAllSettingContentPages().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while closing settings page");
            _toastMessageService.ShowError(exception.Message);
        }
    }

    private async Task OnDisappearingForAllSettingContentPages()
    {
        foreach (SettingsContentPageViewModel contentPageViewModel in _viewModel.SettingsContentPages.Select(x => x.ViewModel))
        {
            await contentPageViewModel.OnDisappearing().ConfigureAwait(false);
        }
    }
}
