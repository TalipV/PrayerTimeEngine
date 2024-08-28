using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class PrayerTimeViewModel
    {
        public PrayerTimeViewModel(
            MainPageViewModel mainPageViewModel, 
            Profile profile)
        {
            this.MainPageViewModel = mainPageViewModel;
            this.Profile = profile;
        }

        public MainPageViewModel MainPageViewModel { get; set; }
        public Profile Profile { get; set; }
        public PrayerTimesBundle PrayerTimeBundle { get; set; }
    }
}
