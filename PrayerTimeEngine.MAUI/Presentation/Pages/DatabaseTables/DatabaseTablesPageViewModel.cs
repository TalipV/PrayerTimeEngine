using Microsoft.EntityFrameworkCore;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Presentation.Pages.Settings.SettingsHandler;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.Pages.DatabaseTables;

public class DatabaseTablesPageViewModel(
        AppDbContext appDbContext
    ) : CustomBaseViewModel
{
    private readonly Dictionary<string, List<object>> _dataDict = [];

    public override void Initialize(params object[] parameter)
    {
        _dataDict[nameof(AppDbContext.FaziletCountries)] = appDbContext.FaziletCountries.AsNoTracking().ToList().OfType<object>().ToList();
        _dataDict[nameof(AppDbContext.FaziletCities)] = appDbContext.FaziletCities.AsNoTracking().ToList().OfType<object>().ToList();
        _dataDict[nameof(AppDbContext.FaziletPrayerTimes)] = appDbContext.FaziletPrayerTimes.AsNoTracking().ToList().OfType<object>().ToList();
        _dataDict[nameof(AppDbContext.SemerkandCountries)] = appDbContext.SemerkandCountries.AsNoTracking().ToList().OfType<object>().ToList();
        _dataDict[nameof(AppDbContext.SemerkandCities)] = appDbContext.SemerkandCities.AsNoTracking().ToList().OfType<object>().ToList();
        _dataDict[nameof(AppDbContext.SemerkandPrayerTimes)] = appDbContext.SemerkandPrayerTimes.AsNoTracking().ToList().OfType<object>().ToList();
        _dataDict[nameof(AppDbContext.MuwaqqitPrayerTimes)] = appDbContext.MuwaqqitPrayerTimes.AsNoTracking().ToList().OfType<object>().ToList();
        _dataDict[nameof(AppDbContext.Profiles)] = appDbContext.Profiles.AsNoTracking().ToList().OfType<object>().ToList();
        _dataDict[nameof(AppDbContext.ProfileConfigs)] = appDbContext.ProfileConfigs.AsNoTracking().ToList().OfType<object>().ToList();
        _dataDict[nameof(AppDbContext.ProfileLocations)] = appDbContext.ProfileLocations.AsNoTracking().ToList().OfType<object>().ToList();

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
}
