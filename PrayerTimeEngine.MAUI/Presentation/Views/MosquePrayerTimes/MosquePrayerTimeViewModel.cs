using NodaTime;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Pages.Main;
using PropertyChanged;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

namespace PrayerTimeEngine.Presentation.Views.MosquePrayerTimes;

[AddINotifyPropertyChangedInterface]
public class MosquePrayerTimeViewModel(
        IMosquePrayerTimeProviderManager mosquePrayerTimeProviderManager
    ) : IPrayerTimeViewModel
{
    public MainPageViewModel MainPageViewModel { get; set; }
    public Profile Profile { get; set; }
    public IPrayerTimesSet PrayerTimesSet { get; set; }

    public AbstractPrayerTime GetDisplayPrayerTime(Instant instant)
    {
        if (PrayerTimesSet is not MosquePrayerTimesSet mosquePrayerTimesSet)
        {
            return null;
        }

        // TODO
        return null;
    }

    public async Task RefreshData(ZonedDateTime zonedDateTime, CancellationToken cancellationToken)
    {
        PrayerTimesSet =
            await mosquePrayerTimeProviderManager.CalculatePrayerTimesAsync(
                Profile.ID,
                zonedDateTime,
                cancellationToken);
    }
}
