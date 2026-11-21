using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;

public class MyMosqRepository(
        IDbContextFactory<AppDbContext> dbContextFactory
    ) : IMyMosqRepository, IPrayerTimeCacheCleaner
{
    private static readonly Func<AppDbContext, LocalDate, string, IAsyncEnumerable<MyMosqMosqueDailyPrayerTimes>> s_compiledQuery_GetPrayerTimesAsync =
        EF.CompileAsyncQuery(
            (AppDbContext context, LocalDate date, string externalID) =>
                context.MyMosqPrayerTimes.AsNoTracking()
            .Where(x => x.Date == date && x.ExternalID == externalID));

    public async Task<MyMosqMosqueDailyPrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken)
    {
        using AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await s_compiledQuery_GetPrayerTimesAsync(dbContext, date, externalID)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task InsertPrayerTimesAsync(List<MyMosqMosqueDailyPrayerTimes> prayerTimesLst, CancellationToken cancellationToken)
    {
        List<MyMosqMosqueDailyPrayerTimes> toBeInserted = [.. prayerTimesLst];

        if (toBeInserted.Count == 0)
        {
            return;
        }

        using AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // the requested days may overlap with days stored by an earlier request (e.g. when looking at
        // other days in the app), so the older entries of these days are replaced by the new ones
        foreach (IGrouping<string, MyMosqMosqueDailyPrayerTimes> group in toBeInserted.GroupBy(x => x.ExternalID))
        {
            string externalID = group.Key;
            LocalDate minDate = group.Min(x => x.Date);
            LocalDate maxDate = group.Max(x => x.Date);

            List<MyMosqMosqueDailyPrayerTimes> overlapping = await dbContext.MyMosqPrayerTimes
                .Where(x => x.ExternalID == externalID)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            dbContext.MyMosqPrayerTimes.RemoveRange(overlapping.Where(x => minDate <= x.Date && x.Date <= maxDate));
        }

        await dbContext.MyMosqPrayerTimes.AddRangeAsync(toBeInserted, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteCacheDataAsync(Instant insertedBefore, CancellationToken cancellationToken)
    {
        using AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.MyMosqPrayerTimes
            .Where(p => p.InsertInstant == null || p.InsertInstant < insertedBefore)
            .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
    }
}
