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

    private class GenericPrayerTime : AbstractPrayerTime
    {
        public override string Name => "Generic";
    }

    public AbstractPrayerTime GetDisplayPrayerTime(ZonedDateTime zonedDateTime)
    {
        LocalTime localTime = zonedDateTime.LocalDateTime.TimeOfDay;

        if (PrayerTimesSet is not MosquePrayerTimesSet mosquePrayerTimesSet)
        {
            return null;
        }

        if (mosquePrayerTimesSet is null)
        {
            return null;
        }

        var currentPrayerTime = mosquePrayerTimesSet.AllPrayerTimes.FirstOrDefault(x => x.Start <= localTime && localTime <= x.End)
            ?? mosquePrayerTimesSet.AllPrayerTimes.OrderBy(x => x.Start).FirstOrDefault(x => x.Start > localTime);

        if (currentPrayerTime == null)
            return null;

        return new GenericPrayerTime
        {
            Start = (zonedDateTime.LocalDateTime.Date + currentPrayerTime.Start).InZoneStrictly(zonedDateTime.Zone),
            End = (zonedDateTime.LocalDateTime.Date + currentPrayerTime.End).InZoneStrictly(zonedDateTime.Zone),
        };
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
