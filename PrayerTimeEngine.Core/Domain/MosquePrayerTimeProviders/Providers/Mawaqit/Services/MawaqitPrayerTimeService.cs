using NodaTime;
using AsyncKeyedLock;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers.Mawaqit.Models.Entities;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers.Mawaqit.Interfaces;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers.Mawaqit.Services
{
    public class MawaqitPrayerTimeService(
            IMawaqitDBAccess mawaqitDBAccess,
            IMawaqitApiService mawaqitApiService
    ) : IMosquePrayerTimeProvider
    {
        private static readonly AsyncKeyedLocker<string> getPrayerTimesLocker = new(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

        internal const int MAX_EXTENT_OF_RETRIEVED_DAYS = 5;

        public async Task<IMosquePrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken)
        {
            using (await getPrayerTimesLocker.LockAsync(externalID, cancellationToken).ConfigureAwait(false))
            {
                MawaqitPrayerTimes prayerTimes = await mawaqitDBAccess.GetPrayerTimesAsync(date, externalID, cancellationToken).ConfigureAwait(false);

                if (prayerTimes is null)
                {
                    var responseDto = await mawaqitApiService.GetPrayerTimesAsync(externalID, cancellationToken);

                    List<MawaqitPrayerTimes> prayerTimesLst =
                        responseDto.ToMawaqitPrayerTimes(externalID)
                        .Where(x => date <= x.Date && x.Date < date.PlusDays(MAX_EXTENT_OF_RETRIEVED_DAYS))
                        .ToList();

                    await mawaqitDBAccess.InsertPrayerTimesAsync(prayerTimesLst, cancellationToken).ConfigureAwait(false);
                    prayerTimes = prayerTimesLst.FirstOrDefault(x => x.Date == date)
                        ?? throw new Exception($"Prayer times for the {date} could not be found for an unknown reason.");
                }

                return prayerTimes;
            }
        }
    }
}
