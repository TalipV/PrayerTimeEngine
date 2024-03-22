using NodaTime;
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

    private readonly HashSet<Type> validTypes =
    [
        typeof(int), typeof(string), typeof(ZonedDateTime), typeof(LocalDate),
        typeof(GenericSettingConfiguration),
        typeof(BaseLocationData),
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

