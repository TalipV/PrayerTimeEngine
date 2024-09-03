using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Calculators.Mosques
{
    public interface IMosquePrayerTimeService
    {
        Task<IMosquePrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken);
    }
}