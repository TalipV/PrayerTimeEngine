using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces
{
    public interface IMuwaqqitDBAccess
    {
        Task<MuwaqqitPrayerTimes> GetTimesAsync(LocalDate date, decimal longitude, decimal latitude, double fajrDegree, double ishaDegree, double ishtibaqDegree, double asrKarahaDegree);
        Task InsertMuwaqqitPrayerTimesAsync(LocalDate date, string timezone, decimal longitude, decimal latitude, double fajrDegree, double ishaDegree, double ishtibaqDegree, double asrKarahaDegree, MuwaqqitPrayerTimes prayerTimes);
    }
}
