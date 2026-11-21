using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Services;

public class MawaqitRepository(
        IDbContextFactory<AppDbContext> dbContextFactory
    ) : IMawaqitRepository, IPrayerTimeCacheCleaner
{
    private static readonly Func<AppDbContext, LocalDate, string, IAsyncEnumerable<MawaqitMosqueDailyPrayerTimes>> s_compiledQuery_GetPrayerTimesAsync =
        EF.CompileAsyncQuery(
            (AppDbContext context, LocalDate date, string externalID) =>
                context.MawaqitPrayerTimes.AsNoTracking()
                .Where(x => x.Date == date && x.ExternalID == externalID));

    public async Task<MawaqitMosqueDailyPrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken)
    {
        using AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await s_compiledQuery_GetPrayerTimesAsync(dbContext, date, externalID)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task InsertPrayerTimesAsync(List<MawaqitMosqueDailyPrayerTimes> prayerTimesLst, CancellationToken cancellationToken)
    {
        List<MawaqitMosqueDailyPrayerTimes> toBeInserted = [.. prayerTimesLst];

        if (toBeInserted.Count == 0)
        {
            return;
        }

        using AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // the requested days may overlap with days stored by an earlier request (e.g. when looking at
        // other days in the app), so the older entries of these days are replaced by the new ones
        foreach (IGrouping<string, MawaqitMosqueDailyPrayerTimes> group in toBeInserted.GroupBy(x => x.ExternalID))
        {
            string externalID = group.Key;
            LocalDate minDate = group.Min(x => x.Date);
            LocalDate maxDate = group.Max(x => x.Date);

            List<MawaqitMosqueDailyPrayerTimes> overlapping = await dbContext.MawaqitPrayerTimes
                .Where(x => x.ExternalID == externalID)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            dbContext.MawaqitPrayerTimes.RemoveRange(overlapping.Where(x => minDate <= x.Date && x.Date <= maxDate));
        }

        await dbContext.MawaqitPrayerTimes.AddRangeAsync(toBeInserted, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteCacheDataAsync(Instant insertedBefore, CancellationToken cancellationToken)
    {
        using AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.MawaqitPrayerTimes
            .Where(p => p.InsertInstant == null || p.InsertInstant < insertedBefore)
            .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
    }
}
