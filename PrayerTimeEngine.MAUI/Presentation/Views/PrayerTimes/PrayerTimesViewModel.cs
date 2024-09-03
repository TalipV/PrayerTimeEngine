using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Pages.Main;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.Views.PrayerTimes
{
    [AddINotifyPropertyChangedInterface]
    public class PrayerTimeViewModel
    {
        public PrayerTimeViewModel(
            MainPageViewModel mainPageViewModel,
            Profile profile)
        {
            MainPageViewModel = mainPageViewModel;
            Profile = profile;
        }

        public MainPageViewModel MainPageViewModel { get; set; }
        public Profile Profile { get; set; }
        public PrayerTimesBundle PrayerTimeBundle { get; set; }
    }
}
