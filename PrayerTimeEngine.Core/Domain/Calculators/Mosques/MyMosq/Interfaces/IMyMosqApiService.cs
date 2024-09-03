using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.MyMosq.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.Calculators.Mosques.MyMosq.Interfaces
{
    public interface IMyMosqApiService
    {
        Task<List<MyMosqPrayerTimesDTO>> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken);
    }
}
