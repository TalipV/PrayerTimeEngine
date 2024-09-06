using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Interfaces
{
    public interface IMuwaqqitDBAccess
    {
        Task<MuwaqqitDailyPrayerTimes> GetPrayerTimesAsync(
            ZonedDateTime date,
            decimal longitude,
            decimal latitude,
            double fajrDegree,
            double ishaDegree,
            double ishtibaqDegree,
            double asrKarahaDegree, CancellationToken cancellationToken);

        Task InsertPrayerTimesAsync(IEnumerable<MuwaqqitDailyPrayerTimes> muwaqqitPrayerTimesLst, CancellationToken cancellationToken);
    }
}
