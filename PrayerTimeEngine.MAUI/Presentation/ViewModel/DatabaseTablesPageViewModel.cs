using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;

namespace PrayerTimeEngine.Presentation.ViewModel
{
    public class DatabaseTablesPageViewModel(
            AppDbContext appDbContext
        ) : CustomBaseViewModel
    {
        public List<FaziletCountry> FaziletCountries { get; private set; } = [];
        public List<FaziletCity> FaziletCities { get; private set; } = [];
        public List<FaziletPrayerTimes> FaziletPrayerTimes { get; private set; } = [];

        public List<SemerkandCountry> SemerkandCountries { get; private set; } = [];
        public List<SemerkandCity> SemerkandCities { get; private set; } = [];
        public List<SemerkandPrayerTimes> SemerkandPrayerTimes { get; private set; } = [];

        public List<MuwaqqitPrayerTimes> MuwaqqitPrayerTimes { get; private set; } = [];

        public List<Profile> Profiles { get; private set; } = [];
        public List<ProfileTimeConfig> ProfileTimeConfigs { get; private set; } = [];
        public List<ProfileLocationConfig> ProfileLocationConfigs { get; private set; } = [];

        public override void Initialize(params object[] parameter)
        {
            FaziletCountries = appDbContext.FaziletCountries.ToList();
            FaziletCities = appDbContext.FaziletCities.ToList();
            FaziletPrayerTimes = appDbContext.FaziletPrayerTimes.ToList();

            SemerkandCountries = appDbContext.SemerkandCountries.ToList();
            SemerkandCities = appDbContext.SemerkandCities.ToList();
            SemerkandPrayerTimes = appDbContext.SemerkandPrayerTimes.ToList();

            MuwaqqitPrayerTimes = appDbContext.MuwaqqitPrayerTimes.ToList();

            Profiles = appDbContext.Profiles.ToList();
            ProfileTimeConfigs = appDbContext.ProfileConfigs.ToList();
            ProfileLocationConfigs = appDbContext.ProfileLocations.ToList();
        }
    }
}
