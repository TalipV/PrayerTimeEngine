using NodaTime;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Pages.Main;
using PropertyChanged;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;

namespace PrayerTimeEngine.Presentation.Views.MosquePrayerTime;

[AddINotifyPropertyChangedInterface]
public class MosquePrayerTimeViewModel(
        IMosquePrayerTimeProviderManager mosquePrayerTimeProviderManager
    ) : IPrayerTimeViewModel
{
    public MainPageViewModel MainPageViewModel { get; set; }
    public Profile Profile { get; set; }
    public PrayerTimesCollection PrayerTimesCollection { get; set; }

    public AbstractPrayerTime GetDisplayPrayerTime(Instant instant)
    {
        var prayerTimeBundle = this.PrayerTimesCollection;

        if (prayerTimeBundle is null)
        {
            return null;
        }

        return prayerTimeBundle.AllPrayerTimes.FirstOrDefault(x => x.Start?.ToInstant() <= instant && instant <= x.End?.ToInstant())
            ?? prayerTimeBundle.AllPrayerTimes.OrderBy(x => x.Start?.ToInstant()).FirstOrDefault(x => x.Start?.ToInstant() > instant);
    }

    public async Task RefreshData(ZonedDateTime zonedDateTime, CancellationToken cancellationToken)
    {
        PrayerTimesCollection =
            await mosquePrayerTimeProviderManager.CalculatePrayerTimesAsync(
                Profile.ID,
                zonedDateTime,
                cancellationToken);
    }
}
