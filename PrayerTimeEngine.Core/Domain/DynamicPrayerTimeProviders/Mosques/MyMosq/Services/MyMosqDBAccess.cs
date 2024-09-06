using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.MyMosq.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.MyMosq.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.MyMosq.Services
{
    public class MyMosqDBAccess(
            IDbContextFactory<AppDbContext> dbContextFactory
        ) : IMyMosqDBAccess, IPrayerTimeCacheCleaner
    {
        private static readonly Func<AppDbContext, LocalDate, string, IAsyncEnumerable<MyMosqPrayerTimes>> compiledQuery_GetPrayerTimesAsync =
            EF.CompileAsyncQuery(
                (AppDbContext context, LocalDate date, string externalID) =>
                    context.MyMosqPrayerTimes.AsNoTracking()
                .Where(x => x.Date == date && x.ExternalID == externalID));

        public async Task<MyMosqPrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await compiledQuery_GetPrayerTimesAsync(dbContext, date, externalID)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task InsertPrayerTimesAsync(List<MyMosqPrayerTimes> prayerTimesLst, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                await dbContext.MyMosqPrayerTimes.AddRangeAsync(prayerTimesLst, cancellationToken).ConfigureAwait(false);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task DeleteCacheDataAsync(ZonedDateTime deleteBeforeDate, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                await dbContext.MyMosqPrayerTimes
                    .Where(p => p.Date < deleteBeforeDate.LocalDateTime.Date)
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
