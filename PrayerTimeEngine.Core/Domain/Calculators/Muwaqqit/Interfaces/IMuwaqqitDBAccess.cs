using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces
{
    public interface IMuwaqqitDBAccess
    {
        Task<MuwaqqitPrayerTimes> GetTimesAsync(
            ZonedDateTime date, 
            decimal longitude, 
            decimal latitude, 
            double fajrDegree, 
            double ishaDegree, 
            double ishtibaqDegree, 
            double asrKarahaDegree, CancellationToken cancellationToken);
        Task InsertMuwaqqitPrayerTimesAsync(IEnumerable<MuwaqqitPrayerTimes> muwaqqitPrayerTimesLst, CancellationToken cancellationToken);
    }
}
