using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.MyMosq.Interfaces;
using AsyncKeyedLock;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.MyMosq.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.Calculators.Mosques.MyMosq.Services
{
    public class MyMosqPrayerTimeService(
        IMyMosqDBAccess myMosqDBAccess,
        IMyMosqApiService myMosqApiService
    ) : IMosquePrayerTimeService
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
                MyMosqPrayerTimes prayerTimes = await myMosqDBAccess.GetPrayerTimesAsync(date, externalID, cancellationToken).ConfigureAwait(false);

                if (prayerTimes is null)
                {
                    var responseDto = await myMosqApiService.GetPrayerTimesAsync(date, externalID, cancellationToken);

                    List<MyMosqPrayerTimes> prayerTimesLst = responseDto
                        .Select(x => x.ToMyMosqPrayerTimes(externalID))
                        .Where(x => date <= x.Date && x.Date < date.PlusDays(MAX_EXTENT_OF_RETRIEVED_DAYS))
                        .ToList();

                    await myMosqDBAccess.InsertPrayerTimesAsync(prayerTimesLst, cancellationToken).ConfigureAwait(false);
                    prayerTimes = prayerTimesLst.FirstOrDefault(x => x.Date == date)
                        ?? throw new Exception($"Prayer times for the {date} could not be found for an unknown reason.");
                }

                return prayerTimes;
            }
        }
    }
}
