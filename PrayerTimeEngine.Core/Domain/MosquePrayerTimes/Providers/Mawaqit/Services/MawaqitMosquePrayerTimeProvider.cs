using AsyncKeyedLock;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Services;

public class MawaqitMosquePrayerTimeProvider(
        IMawaqitDBAccess mawaqitDBAccess,
        IMawaqitApiService mawaqitApiService,
        ISystemInfoService systemInfoService
) : IMosquePrayerTimeProvider
{
    private static readonly AsyncKeyedLocker<string> getPrayerTimesLocker = new(o =>
    {
        o.PoolSize = 20;
        o.PoolInitialFill = 1;
    });

    internal const int MAX_EXTENT_OF_RETRIEVED_DAYS = 5;

    public async Task<IMosqueDailyPrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken)
    {
        using (await getPrayerTimesLocker.LockAsync(externalID, cancellationToken).ConfigureAwait(false))
        {
            MawaqitMosqueDailyPrayerTimes prayerTimes = await mawaqitDBAccess.GetPrayerTimesAsync(date, externalID, cancellationToken).ConfigureAwait(false);

            if (prayerTimes is null)
            {
                var responseDto = await mawaqitApiService.GetPrayerTimesAsync(externalID, cancellationToken);

                int currentYear = systemInfoService.GetCurrentZonedDateTime().Year;

                List<MawaqitMosqueDailyPrayerTimes> prayerTimesLst = responseDto.ToMawaqitPrayerTimes(currentYear, externalID)
                    .Where(x => date <= x.Date && x.Date < date.PlusDays(MAX_EXTENT_OF_RETRIEVED_DAYS))
                    .ToList();

                await mawaqitDBAccess.InsertPrayerTimesAsync(prayerTimesLst, cancellationToken).ConfigureAwait(false);
                prayerTimes = prayerTimesLst.FirstOrDefault(x => x.Date == date)
                    ?? throw new Exception($"Prayer times for the {date} could not be found for an unknown reason.");
            }

            return prayerTimes;
        }
    }

    public Task<bool> ValidateData(string externalID, CancellationToken cancellationToken)
    {
        return mawaqitApiService.ValidateData(externalID, cancellationToken);
    }
}
