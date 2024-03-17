using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces
{
    public interface IMuwaqqitDBAccess
    {
        Task<MuwaqqitPrayerTimes> GetTimesAsync(LocalDate date, decimal longitude, decimal latitude, double fajrDegree, double ishaDegree, double ishtibaqDegree, double asrKarahaDegree, CancellationToken cancellationToken);
        Task InsertMuwaqqitPrayerTimesAsync(MuwaqqitPrayerTimes prayerTimes, CancellationToken cancellationToken);
    }
}
