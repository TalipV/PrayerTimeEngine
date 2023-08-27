using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Interfaces
{
    public interface IMuwaqqitApiService
    {
        Task<MuwaqqitPrayerTimes> GetTimesAsync(DateTime date, decimal longitude, decimal latitude, double fajrDegree, double ishaDegree, double ishtibaqDegree, double asrKarahaDegree, string timezone);
    }
}
