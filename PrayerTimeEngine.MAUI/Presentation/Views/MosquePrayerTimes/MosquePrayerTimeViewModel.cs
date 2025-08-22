using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.Views.MosquePrayerTimes;

[AddINotifyPropertyChangedInterface]
public class MosquePrayerTimeViewModel(
        IMosquePrayerTimeProviderManager mosquePrayerTimeProviderManager,
        MosqueProfile profile
    ) : BasePrayerTimeViewModel<MosqueProfile, MosquePrayerTimesDay>(profile)
{
    public override Task<MosquePrayerTimesDay> GetPrayerTimesSet(ZonedDateTime zonedDateTime, CancellationToken cancellationToken)
    {
        return mosquePrayerTimeProviderManager.CalculatePrayerTimesAsync(
                ProfileActual.ID,
                zonedDateTime,
                cancellationToken);
    }
}
