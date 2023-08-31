using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces
{
    public interface IMuwaqqitApiService
    {
        Task<MuwaqqitPrayerTimes> GetTimesAsync(LocalDate date, decimal longitude, decimal latitude, double fajrDegree, double ishaDegree, double ishtibaqDegree, double asrKarahaDegree, string timezone);
    }
}
