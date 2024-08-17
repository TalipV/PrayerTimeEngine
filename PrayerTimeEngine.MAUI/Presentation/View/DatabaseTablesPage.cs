using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Presentation.ViewModel;
using System.Reflection;
using UraniumUI.Material.Controls;

namespace PrayerTimeEngine.Presentation.View;

public partial class DatabaseTablesPage : ContentPage
{
    private readonly DataGrid _dataGrid;

    public DatabaseTablesPage(DatabaseTablesPageViewModel viewModel)
    {
        BindingContext = viewModel;

        var picker = new Picker();
        picker.SetBinding(Picker.ItemsSourceProperty, "TableOptions");
        picker.SetBinding(Picker.SelectedItemProperty, "SelectedTableOption");
        NavigationPage.SetTitleView(this, picker);

        Content = new ScrollView
        {
            Orientation = ScrollOrientation.Both,
            Margin = new Thickness(10),
            Content = _dataGrid = new DataGrid()
        };

        viewModel.OnChangeSelectionAction = PopulateTabViewWithItems;
    }

    // Maybe adding some attribute to exclude specific properties would be a more intuitive option?
    // But it would only be for this specific debug feature so here like this is fine, I guess.
    private readonly HashSet<Type> validTypes =
    [
        typeof(string),
        typeof(int), typeof(int?), 
        typeof(double), typeof(double?), 
        typeof(decimal), typeof(decimal?), 
        typeof(ZonedDateTime), typeof(ZonedDateTime?),
        typeof(LocalTime), typeof(LocalTime?),
        typeof(LocalDate), typeof(LocalDate?),
        typeof(Instant), typeof(Instant?),
        typeof(GenericSettingConfiguration),
        typeof(BaseLocationData),
        typeof(ECalculationSource),
        typeof(ETimeType),
    ];

    public void PopulateTabViewWithItems(List<object> list)
    {
        if (list.Count == 0)
            return;

        _dataGrid.ItemsSource = null;
        _dataGrid.Columns.Clear();

        List<PropertyInfo> propertyInfos =
            list.First().GetType().GetProperties()
                .Where(x => validTypes.Contains(x.PropertyType))
                .ToList();

        foreach (PropertyInfo prop in propertyInfos)
        {
            // TODO FIX... someday
#pragma warning disable CS0618 // Type or member is obsolete
            var dataGridColumn = new DataGridColumn
            {
                Title = prop.Name,
                Binding = new Binding(prop.Name)
            };
#pragma warning restore CS0618 // Type or member is obsolete
            _dataGrid.Columns.Add(dataGridColumn);
        }

        _dataGrid.ItemsSource = list;
    }
}

