using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers.Mawaqit.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers.Mawaqit.Interfaces
{
    public interface IMawaqitDBAccess
    {
        Task<MawaqitPrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken);
        Task InsertPrayerTimesAsync(List<MawaqitPrayerTimes> prayerTimesLst, CancellationToken cancellationToken);
    }
}
