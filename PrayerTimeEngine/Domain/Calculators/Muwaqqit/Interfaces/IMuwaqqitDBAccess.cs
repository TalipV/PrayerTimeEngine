using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Interfaces
{
    public interface IMuwaqqitDBAccess
    {
        Task<MuwaqqitPrayerTimes> GetTimesAsync(DateTime date, decimal longitude, decimal latitude, double fajrDegree, double ishaDegree, double ishtibaqDegree, double asrKarahaDegree);
        Task InsertMuwaqqitPrayerTimesAsync(DateTime date, string timezone, decimal longitude, decimal latitude, double fajrDegree, double ishaDegree, double ishtibaqDegree, double asrKarahaDegree, MuwaqqitPrayerTimes prayerTimes);
    }
}
