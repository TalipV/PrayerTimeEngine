using Microsoft.Maui.Controls.Shapes;
using PrayerTimeEngine.Presentation.ViewModel;

namespace PrayerTimeEngine.Code.Presentation.View;

public partial class SettingsContentPage : ContentPage
{
    public SettingsContentPageViewModel ViewModel { get; private set; }

    private readonly StackLayout _configurableUIContainer;

    private readonly Picker _calculationSourcePicker;
    private readonly Picker _minuteAdjustmentPicker;

    private readonly Label _calculationSourcePickerLabel;
    private readonly Label _isTimeShownCheckBoxLabel;
    private readonly CheckBox _isTimeShownCheckBox;

    public SettingsContentPage(SettingsContentPageViewModel viewModel)
    {
        var grid = new Grid
        {
            Padding = new Thickness(15),
            RowDefinitions =
                [
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(4, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(4, GridUnitType.Star) }
                ],
            ColumnDefinitions =
                [
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                ]
        };

        _calculationSourcePickerLabel = new Label { Text = "Calculation Source" };
        _calculationSourcePicker = new Picker();

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

        grid.AddWithSpan(view: _calculationSourcePickerLabel);
        grid.AddWithSpan(view: _calculationSourcePicker, row: 1, column: 0);
        grid.AddWithSpan(view: minuteAdjustmentLabel, column: 2, columnSpan: 3);
        grid.AddWithSpan(view: _minuteAdjustmentPicker, row: 1, column: 2);
        grid.AddWithSpan(view: _isTimeShownCheckBoxLabel, row: 2, columnSpan: 3);
        grid.AddWithSpan(view: _isTimeShownCheckBox, row: 3, column: 0);
        grid.AddWithSpan(view: line, row: 4, columnSpan: 4);
        grid.AddWithSpan(view: _configurableUIContainer, row: 5, columnSpan: 2);

        Content = grid;

        ViewModel = viewModel;
        BindingContext = ViewModel;

        ViewModel.OnInitializeCustomUI_EventTrigger += () => onInitializeCustomUI_EventTrigger();
        ViewModel.OnViewModelInitialize_EventTrigger += onViewModelInitialize_EventTrigger;
    }

    private void onViewModelInitialize_EventTrigger()
    {
        addDataBindings();
    }

    private void onInitializeCustomUI_EventTrigger()
    {
        configureCustomUISection();
    }

    private void configureCustomUISection()
    {
        _configurableUIContainer.Children.Clear();

        if (ViewModel.CustomSettingConfigurationViewModel == null)
        {
            return;
        }

        if (ViewModel.CustomSettingConfigurationViewModel.GetUI() is IView customSettingConfiguration)
        {
            _configurableUIContainer.Children.Add(customSettingConfiguration);
        }
    }

    private void addDataBindings()
    {
        this._calculationSourcePicker.SetBinding(Picker.ItemsSourceProperty, nameof(SettingsContentPageViewModel.CalculationSources), BindingMode.Default);
        this._calculationSourcePickerLabel.SetBinding(Label.IsVisibleProperty, nameof(SettingsContentPageViewModel.ShowCalculationSourcePicker), BindingMode.Default);
        this._calculationSourcePicker.SetBinding(Picker.SelectedItemProperty, nameof(SettingsContentPageViewModel.SelectedCalculationSource), BindingMode.TwoWay);
        this._calculationSourcePicker.SetBinding(Picker.IsVisibleProperty, nameof(SettingsContentPageViewModel.ShowCalculationSourcePicker), BindingMode.Default);

        this._minuteAdjustmentPicker.SetBinding(Picker.ItemsSourceProperty, nameof(SettingsContentPageViewModel.MinuteAdjustments), BindingMode.Default);
        this._minuteAdjustmentPicker.SetBinding(Picker.SelectedItemProperty, nameof(SettingsContentPageViewModel.SelectedMinuteAdjustment), BindingMode.TwoWay);

        this._isTimeShownCheckBoxLabel.SetBinding(Label.IsVisibleProperty, nameof(SettingsContentPageViewModel.IsTimeShownCheckBoxVisible), BindingMode.Default);
        this._isTimeShownCheckBox.SetBinding(CheckBox.IsCheckedProperty, nameof(SettingsContentPageViewModel.IsTimeShown), BindingMode.TwoWay);
        this._isTimeShownCheckBox.SetBinding(CheckBox.IsVisibleProperty, nameof(SettingsContentPageViewModel.IsTimeShownCheckBoxVisible), BindingMode.Default);
    }
}