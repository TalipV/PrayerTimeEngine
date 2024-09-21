using Microsoft.Maui.Controls.Shapes;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

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

    public SettingsContentPage(SettingsContentPageViewModel viewModel)
    {
        Content = createUI();

        ViewModel = viewModel;
        BindingContext = ViewModel;

        ViewModel.OnInitializeCustomUI_EventTrigger += onInitializeCustomUI_EventTrigger;
    }

    private Grid createUI()
    {
        var grid = new Grid
        {
            Padding = new Thickness(15),
            RowDefinitions = Rows.Define(
                    GridLength.Auto,
                    GridLength.Auto,
                    GridLength.Auto,
                    GridLength.Auto,
                    GridLength.Star,
                    new GridLength(4, GridUnitType.Star),
                    new GridLength(4, GridUnitType.Star)
                ),
            ColumnDefinitions = Columns.Define(
                    GridLength.Auto,
                    GridLength.Star,
                    GridLength.Star,
                    GridLength.Star
                )
        };

        _dynamicPrayerTimeProviderPickerLabel = new Label { Text = "Calculation Source" };
        _dynamicPrayerTimeProviderPicker = new Picker();

        var minuteAdjustmentLabel = new Label { Text = "Minute Adjustment" };
        _minuteAdjustmentPicker = new Picker();

        _isTimeShownCheckBoxLabel = new Label { Text = "Shown:" };
        _isTimeShownCheckBox = new CheckBox();

        var line = new Line
        {
            X1 = 0,
            Y1 = 10,
            X2 = 350,
            Y2 = 10,
            Stroke = Colors.White
        };

        _configurableUIContainer = [];

        grid.AddWithSpan(view: _dynamicPrayerTimeProviderPickerLabel);
        grid.AddWithSpan(view: _dynamicPrayerTimeProviderPicker, row: 1, column: 0);
        grid.AddWithSpan(view: minuteAdjustmentLabel, column: 2, columnSpan: 3);
        grid.AddWithSpan(view: _minuteAdjustmentPicker, row: 1, column: 2);
        grid.AddWithSpan(view: _isTimeShownCheckBoxLabel, row: 2, columnSpan: 3);
        grid.AddWithSpan(view: _isTimeShownCheckBox, row: 3, column: 0);
        grid.AddWithSpan(view: line, row: 4, columnSpan: 4);
        grid.AddWithSpan(view: _configurableUIContainer, row: 5, columnSpan: 2);

        return grid;
    }

    private void onInitializeCustomUI_EventTrigger()
    {
        configureCustomUISection();
    }

    private void configureCustomUISection()
    {
        _configurableUIContainer.Children.Clear();

        if (ViewModel.CustomSettingConfigurationViewModel is null)
        {
            return;
        }

        if (ViewModel.CustomSettingConfigurationViewModel.GetUI() is IView customSettingConfiguration)
        {
            _configurableUIContainer.Children.Add(customSettingConfiguration);
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
        _isTimeShownCheckBox.SetBinding(IsVisibleProperty, nameof(SettingsContentPageViewModel.IsTimeShownCheckBoxVisible), BindingMode.Default);
    }
}