using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.MyMosq.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.MyMosq.Interfaces
{
    public interface IMyMosqDBAccess
    {
        Task<MyMosqPrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken);
        Task InsertPrayerTimesAsync(List<MyMosqPrayerTimes> prayerTimesLst, CancellationToken cancellationToken);
    }
}
