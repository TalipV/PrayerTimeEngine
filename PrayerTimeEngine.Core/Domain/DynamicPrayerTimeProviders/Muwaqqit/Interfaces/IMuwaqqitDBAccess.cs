using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Muwaqqit.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Muwaqqit.Interfaces
{
    public interface IMuwaqqitDBAccess
    {
        Task<MuwaqqitPrayerTimes> GetPrayerTimesAsync(
            ZonedDateTime date,
            decimal longitude,
            decimal latitude,
            double fajrDegree,
            double ishaDegree,
            double ishtibaqDegree,
            double asrKarahaDegree, CancellationToken cancellationToken);

        Task InsertPrayerTimesAsync(IEnumerable<MuwaqqitPrayerTimes> muwaqqitPrayerTimesLst, CancellationToken cancellationToken);
    }
}
