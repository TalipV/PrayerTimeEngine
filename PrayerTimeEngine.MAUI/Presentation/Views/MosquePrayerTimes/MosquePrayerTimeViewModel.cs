using NodaTime;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PropertyChanged;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

namespace PrayerTimeEngine.Presentation.Views.MosquePrayerTimes;

[AddINotifyPropertyChangedInterface]
public class MosquePrayerTimeViewModel(
        IMosquePrayerTimeProviderManager mosquePrayerTimeProviderManager,
        MosqueProfile profile
    ) : BasePrayerTimeViewModel<MosqueProfile, MosquePrayerTimesSet>(profile)
{
    public override Task<MosquePrayerTimesSet> GetPrayerTimesSet(ZonedDateTime zonedDateTime, CancellationToken cancellationToken)
    {
        return mosquePrayerTimeProviderManager.CalculatePrayerTimesAsync(
                ProfileActual.ID,
                zonedDateTime,
                cancellationToken);
    }
}
