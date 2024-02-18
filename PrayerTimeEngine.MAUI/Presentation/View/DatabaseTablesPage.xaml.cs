using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;
using PrayerTimeEngine.Presentation.ViewModel;
using System.Reflection;
using UraniumUI.Material.Controls;

namespace PrayerTimeEngine.Views;

public partial class DatabaseTablesPage : ContentPage
{
    private DatabaseTablesPageViewModel ViewModel { get; }

    public DatabaseTablesPage(DatabaseTablesPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = ViewModel = viewModel;

        this.Appearing += DatabaseTablesPage_Appearing;
    }

    private void DatabaseTablesPage_Appearing(object sender, EventArgs e)
    {
        PopulateTabViewWithItems(
            [
                (nameof(DatabaseTablesPageViewModel.Profiles), typeof(Profile)),
                (nameof(DatabaseTablesPageViewModel.ProfileTimeConfigs), typeof(ProfileTimeConfig)),
                (nameof(DatabaseTablesPageViewModel.ProfileLocationConfigs), typeof(ProfileLocationConfig)),

                (nameof(DatabaseTablesPageViewModel.FaziletCountries), typeof(FaziletCountry)),
                (nameof(DatabaseTablesPageViewModel.FaziletCities), typeof(FaziletCity)),
                (nameof(DatabaseTablesPageViewModel.FaziletPrayerTimes), typeof(FaziletPrayerTimes)),
                
                (nameof(DatabaseTablesPageViewModel.SemerkandCountries), typeof(SemerkandCountry)),
                (nameof(DatabaseTablesPageViewModel.SemerkandCities), typeof(SemerkandCity)),
                (nameof(DatabaseTablesPageViewModel.SemerkandPrayerTimes), typeof(SemerkandPrayerTimes)),

                (nameof(DatabaseTablesPageViewModel.MuwaqqitPrayerTimes), typeof(MuwaqqitPrayerTimes)),
            ]);
    }

    private readonly HashSet<Type> validTypes =
    [
        typeof(int), typeof(string), typeof(ZonedDateTime), typeof(LocalDate),
        typeof(GenericSettingConfiguration),
        typeof(BaseLocationData),
    ];

    public void PopulateTabViewWithItems(List<(string bindingPropertyName, Type type)> itemsLists)
    {
        foreach ((string bindingPropertyName, Type type) in itemsLists)
        {
            var dataGrid = new DataGrid
            {
                BindingContext = this.ViewModel
            };
            dataGrid.SetBinding(DataGrid.ItemsSourceProperty, bindingPropertyName);

            var scrollView = new ScrollView
            {
                Orientation = ScrollOrientation.Both,
                Margin = 10,
                Content = dataGrid
            };
            tabView.Items.Add(new TabItem
            {
                Content = scrollView,
                Title = bindingPropertyName,
            });

            var propertieInfos = 
                type.GetProperties()
                    .Where(x => validTypes.Contains(x.PropertyType))
                    .Select((x, index) => (x, index))
                    .ToList();

            foreach ((PropertyInfo prop, int index) in propertieInfos)
            {
                var dataGridColumn = new DataGridColumn
                {
                    Title = prop.Name,
                    Binding = new Binding(prop.Name)
                };
                dataGrid.Columns.Add(dataGridColumn);
            }
        }
    }
}

