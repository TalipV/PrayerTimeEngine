using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.ViewModel
{
    public class DatabaseTablesPageViewModel(
            AppDbContext appDbContext
        ) : CustomBaseViewModel
    {
        private readonly Dictionary<string, List<object>> _dataDict = []; 

        public override void Initialize(params object[] parameter)
        {
            _dataDict[nameof(AppDbContext.FaziletCountries)] = appDbContext.FaziletCountries.ToList().OfType<object>().ToList();
            _dataDict[nameof(AppDbContext.FaziletCities)] = appDbContext.FaziletCities.ToList().OfType<object>().ToList();
            _dataDict[nameof(AppDbContext.FaziletPrayerTimes)] = appDbContext.FaziletPrayerTimes.ToList().OfType<object>().ToList();
            _dataDict[nameof(AppDbContext.SemerkandCountries)] = appDbContext.SemerkandCountries.ToList().OfType<object>().ToList();
            _dataDict[nameof(AppDbContext.SemerkandCities)] = appDbContext.SemerkandCities.ToList().OfType<object>().ToList();
            _dataDict[nameof(AppDbContext.SemerkandPrayerTimes)] = appDbContext.SemerkandPrayerTimes.ToList().OfType<object>().ToList();
            _dataDict[nameof(AppDbContext.MuwaqqitPrayerTimes)] = appDbContext.MuwaqqitPrayerTimes.ToList().OfType<object>().ToList();
            _dataDict[nameof(AppDbContext.Profiles)] = appDbContext.Profiles.ToList().OfType<object>().ToList();
            _dataDict[nameof(AppDbContext.ProfileConfigs)] = appDbContext.ProfileConfigs.ToList().OfType<object>().ToList();
            _dataDict[nameof(AppDbContext.ProfileLocations)] = appDbContext.ProfileLocations.ToList().OfType<object>().ToList();

            TableOptions = _dataDict.Select(x => x.Key).ToList();
            SelectedTableOption = TableOptions[0];
        }

        public List<string> TableOptions { get; set; }

        [OnChangedMethod(nameof(onSelectedTableOptionChanged))]
        public string SelectedTableOption { get; set; }

        public Action<List<object>> OnChangeSelectionAction { get; set; }

        private void onSelectedTableOptionChanged()
        {
            if (string.IsNullOrWhiteSpace(this.SelectedTableOption) || !_dataDict.ContainsKey(this.SelectedTableOption))
                return;

            var list = _dataDict[this.SelectedTableOption];

            OnChangeSelectionAction?.Invoke(list);
        }
    }
}
