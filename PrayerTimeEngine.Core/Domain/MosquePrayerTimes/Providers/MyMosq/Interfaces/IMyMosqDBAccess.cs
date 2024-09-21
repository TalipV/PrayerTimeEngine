using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Interfaces;

public interface IMyMosqDBAccess
{
    Task<MyMosqMosqueDailyPrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken);
    Task InsertPrayerTimesAsync(List<MyMosqMosqueDailyPrayerTimes> prayerTimesLst, CancellationToken cancellationToken);
}
