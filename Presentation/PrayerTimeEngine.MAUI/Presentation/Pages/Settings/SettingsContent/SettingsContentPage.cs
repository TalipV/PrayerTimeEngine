using Microsoft.Maui.Controls.Shapes;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;
using PrayerTimeEngine.Presentation.Localization;

namespace PrayerTimeEngine.Presentation.Pages.Settings.SettingsContent;

public partial class SettingsContentPage : ContentPage
{
    public SettingsContentPageViewModel ViewModel { get; private set; }

    private StackLayout _configurableUIContainer;

    private Picker _dynamicPrayerTimeProviderPicker;
    private Picker _minuteAdjustmentPicker;

    private Label _dynamicPrayerTimeProviderPickerLabel;
    private Label _isTimeShownCheckBoxLabel;
    private CheckBox _isTimeShownCheckBox;
    private Grid _isTimeShownRow;

    public SettingsContentPage(SettingsContentPageViewModel viewModel)
    {
        Content = CreateUI();

        ViewModel = viewModel;
        BindingContext = ViewModel;

        ViewModel.OnInitializeCustomUI_EventTrigger += OnInitializeCustomUI_EventTrigger;
    }

    /// <summary>
    /// One card with the general settings of the time and, below it, the settings that only
    /// some calculation sources have (e.g. the Muwaqqit degree).
    /// </summary>
    private VerticalStackLayout CreateUI()
    {
        _dynamicPrayerTimeProviderPickerLabel = CreateFieldLabel(AppResources.Settings_CalculationSource);
        _dynamicPrayerTimeProviderPicker = CreatePicker();

        Label minuteAdjustmentLabel = CreateFieldLabel(AppResources.Settings_MinuteAdjustment);
        _minuteAdjustmentPicker = CreatePicker();

        _isTimeShownCheckBoxLabel = new Label
        {
            Text = AppResources.Settings_Shown,
            TextColor = AppColors.Text,
            FontSize = 15,
            VerticalOptions = LayoutOptions.Center
        };
        _isTimeShownCheckBox = new CheckBox
        {
            Color = AppColors.Text,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center
        };

        var fieldGrid = new Grid
        {
            ColumnSpacing = 16,
            RowSpacing = 2,
            RowDefinitions = Rows.Define(GridLength.Auto, GridLength.Auto),
            ColumnDefinitions = Columns.Define(GridLength.Star, GridLength.Star)
        };
        fieldGrid.Add(_dynamicPrayerTimeProviderPickerLabel, column: 0, row: 0);
        fieldGrid.Add(_dynamicPrayerTimeProviderPicker, column: 0, row: 1);
        fieldGrid.Add(minuteAdjustmentLabel, column: 1, row: 0);
        fieldGrid.Add(_minuteAdjustmentPicker, column: 1, row: 1);

        _isTimeShownRow = new Grid
        {
            ColumnDefinitions = Columns.Define(GridLength.Star, GridLength.Auto)
        };
        _isTimeShownRow.Add(_isTimeShownCheckBoxLabel, column: 0);
        _isTimeShownRow.Add(_isTimeShownCheckBox, column: 1);

        // only shown when a calculation source brings its own settings, otherwise it would add an empty gap to the card
        _configurableUIContainer = new StackLayout { IsVisible = false };

        // no ScrollView: the content is short, and on Android a ScrollView would swallow the
        // horizontal swipe the settings page uses to switch between the tabs
        var card = new Border
        {
            BackgroundColor = AppColors.Card,
            Stroke = AppColors.Card,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = new Thickness(16, 14),
            Content = new VerticalStackLayout
            {
                Spacing = 14,
                Children = { fieldGrid, _isTimeShownRow, _configurableUIContainer }
            }
        };

        return new VerticalStackLayout
        {
            Padding = new Thickness(14, 16),
            Children = { card }
        };
    }

    private static Label CreateFieldLabel(string text)
    {
        return new Label
        {
            Text = text,
            TextColor = AppColors.TextMuted,
            FontSize = 13
        };
    }

    /// <summary>
    /// Colours are set explicitly instead of following the system theme,
    /// because the app is on the fixed dark palette of <see cref="AppColors"/>.
    /// </summary>
    private static Picker CreatePicker()
    {
        return new Picker
        {
            TextColor = AppColors.Text,
            TitleColor = AppColors.Text
        };
    }

    private void OnInitializeCustomUI_EventTrigger()
    {
        ConfigureCustomUISection();
    }

    private void ConfigureCustomUISection()
    {
        _configurableUIContainer.Children.Clear();
        _configurableUIContainer.IsVisible = false;

        if (ViewModel.CustomSettingConfigurationViewModel is null)
        {
            return;
        }

        if (ViewModel.CustomSettingConfigurationViewModel.GetUI() is IView customSettingConfiguration)
        {
            _configurableUIContainer.Children.Add(customSettingConfiguration);
            _configurableUIContainer.IsVisible = true;
        }
    }

    public void AddDataBindings()
    {
        _dynamicPrayerTimeProviderPicker.SetBinding(Picker.ItemsSourceProperty, nameof(SettingsContentPageViewModel.DynamicPrayerTimeProviders), BindingMode.Default);
        _dynamicPrayerTimeProviderPicker.SetBinding(Picker.SelectedItemProperty, nameof(SettingsContentPageViewModel.SelectedDynamicPrayerTimeProvider), BindingMode.TwoWay);
        _dynamicPrayerTimeProviderPickerLabel.SetBinding(IsVisibleProperty, nameof(SettingsContentPageViewModel.ShowDynamicPrayerTimeProviderPicker), BindingMode.Default);
        _dynamicPrayerTimeProviderPicker.SetBinding(IsVisibleProperty, nameof(SettingsContentPageViewModel.ShowDynamicPrayerTimeProviderPicker), BindingMode.Default);

        _minuteAdjustmentPicker.SetBinding(Picker.ItemsSourceProperty, nameof(SettingsContentPageViewModel.MinuteAdjustments), BindingMode.Default);
        _minuteAdjustmentPicker.SetBinding(Picker.SelectedItemProperty, nameof(SettingsContentPageViewModel.SelectedMinuteAdjustment), BindingMode.TwoWay);

        _isTimeShownCheckBoxLabel.SetBinding(IsVisibleProperty, nameof(SettingsContentPageViewModel.IsTimeShownCheckBoxVisible), BindingMode.Default);
        _isTimeShownCheckBox.SetBinding(CheckBox.IsCheckedProperty, nameof(SettingsContentPageViewModel.IsTimeShown), BindingMode.TwoWay);
        _isTimeShownRow.SetBinding(IsVisibleProperty, nameof(SettingsContentPageViewModel.IsTimeShownCheckBoxVisible), BindingMode.Default);
    }
}