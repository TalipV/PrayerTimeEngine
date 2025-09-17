using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Presentation.Pages.Settings.SettingsHandler;
using PropertyChanged;
using System.Reflection;

namespace PrayerTimeEngine.Presentation.Pages.DatabaseTables;

public partial class DatabaseTablesPageViewModel(
        AppDbContext appDbContext
    ) : CustomBaseViewModel
{
    private readonly Dictionary<string, List<object>> _dataDict = [];

    public override void Initialize(params object[] parameter)
    {
        fillDataDict();

        TableOptions = _dataDict.Select(x => x.Key).ToList();
        SelectedTableOption = TableOptions[0];
    }

    public List<string> TableOptions { get; set; }

    [OnChangedMethod(nameof(onSelectedTableOptionChanged))]
    public string SelectedTableOption { get; set; }

    public Action<List<object>> OnChangeSelectionAction { get; set; }

    private void onSelectedTableOptionChanged()
    {
        if (string.IsNullOrWhiteSpace(SelectedTableOption))
            return;

        if (!_dataDict.TryGetValue(SelectedTableOption, out List<object> value))
            return;

        OnChangeSelectionAction?.Invoke(value);
    }

    private void fillDataDict()
    {
        List<PropertyInfo> dbSetPropertyInfos = typeof(AppDbContext)
            .GetProperties()
            .Where(x => x.PropertyType.IsGenericType &&
                        x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .ToList();

        foreach (PropertyInfo dbSetPropertyInfo in dbSetPropertyInfos)
        {
            Type entityTypeOfDbSet = dbSetPropertyInfo.PropertyType.GetGenericArguments()[0];
            MethodInfo setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes);
            MethodInfo genericSet = setMethod.MakeGenericMethod(entityTypeOfDbSet);
            object dbSet = genericSet.Invoke(appDbContext, null);

            var asNoTrackingMethod = typeof(EntityFrameworkQueryableExtensions)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.AsNoTracking)
                                && m.GetParameters().Length == 1)
                    .MakeGenericMethod(entityTypeOfDbSet);
            object dbSetWithAsNoTracking = asNoTrackingMethod.Invoke(null, [dbSet]);

            MethodInfo toListMethod = typeof(Enumerable)
                .GetMethod(nameof(Enumerable.ToList))
                .MakeGenericMethod(entityTypeOfDbSet);

            object dbSetFullResult = toListMethod.Invoke(null, [dbSetWithAsNoTracking]);

            _dataDict[dbSetPropertyInfo.Name] = ((IEnumerable<object>)dbSetFullResult).ToList();
        }
    }

}
