using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Interfaces;

public interface IMawaqitRepository
{
    Task<MawaqitMosqueDailyPrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken);
    Task InsertPrayerTimesAsync(List<MawaqitMosqueDailyPrayerTimes> prayerTimesLst, CancellationToken cancellationToken);
}
