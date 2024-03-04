using NodaTime;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Presentation.ViewModel;
using System.Reflection;
using UraniumUI.Material.Controls;

namespace PrayerTimeEngine.Views;

public partial class DatabaseTablesPage : ContentPage
{
    public DatabaseTablesPage(DatabaseTablesPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

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

        dataGrid.ItemsSource = null;
        dataGrid.Columns.Clear();

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
            dataGrid.Columns.Add(dataGridColumn);
        }

        dataGrid.ItemsSource = list;
    }
}

