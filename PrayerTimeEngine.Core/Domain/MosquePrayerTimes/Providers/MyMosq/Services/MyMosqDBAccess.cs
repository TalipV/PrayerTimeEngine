using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;

public class MyMosqDBAccess(
        IDbContextFactory<AppDbContext> dbContextFactory
    ) : IMyMosqDBAccess, IPrayerTimeCacheCleaner
{
    private static readonly Func<AppDbContext, LocalDate, string, IAsyncEnumerable<MyMosqMosqueDailyPrayerTimes>> compiledQuery_GetPrayerTimesAsync =
        EF.CompileAsyncQuery(
            (AppDbContext context, LocalDate date, string externalID) =>
                context.MyMosqPrayerTimes.AsNoTracking()
            .Where(x => x.Date == date && x.ExternalID == externalID));

    public async Task<MyMosqMosqueDailyPrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken)
    {
        using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            return await compiledQuery_GetPrayerTimesAsync(dbContext, date, externalID)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task InsertPrayerTimesAsync(List<MyMosqMosqueDailyPrayerTimes> prayerTimesLst, CancellationToken cancellationToken)
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
            // TODO fix
            // db side querying doesn't work for some reason
            var toBeDeletedTimes = (await dbContext.MyMosqPrayerTimes.ToListAsync(cancellationToken).ConfigureAwait(false))
                .Where(p => p.Date < deleteBeforeDate.LocalDateTime.Date)
                .ToList();

            dbContext.MyMosqPrayerTimes.RemoveRange(toBeDeletedTimes);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
