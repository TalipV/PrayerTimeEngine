using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Presentation.ViewModel
{
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
    }
}
