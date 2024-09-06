using NodaTime;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Pages.Main;

namespace PrayerTimeEngine.Presentation
{
    public interface IPrayerTimeViewModel
    {
        MainPageViewModel MainPageViewModel { get; set; }
        Profile Profile { get; set; }
        PrayerTimesCollection PrayerTimesCollection { get; set; }

        Task RefreshData(ZonedDateTime zonedDateTime, CancellationToken cancellationToken);
        AbstractPrayerTime GetDisplayPrayerTime(Instant instant);
    }
}
