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
    private static readonly Func<AppDbContext, LocalDate, string, IAsyncEnumerable<MawaqitMosqueDailyPrayerTimes>> compiledQuery_GetPrayerTimesAsync =
        EF.CompileAsyncQuery(
            (AppDbContext context, LocalDate date, string externalID) =>
                context.MawaqitPrayerTimes.AsNoTracking()
                .Where(x => x.Date == date && x.ExternalID == externalID));

    public async Task<MawaqitMosqueDailyPrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken)
    {
        using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            return await compiledQuery_GetPrayerTimesAsync(dbContext, date, externalID)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task InsertPrayerTimesAsync(List<MawaqitMosqueDailyPrayerTimes> prayerTimesLst, CancellationToken cancellationToken)
    {
        using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            await dbContext.MawaqitPrayerTimes.AddRangeAsync(prayerTimesLst, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task DeleteCacheDataAsync(LocalDate deleteBeforeDate, CancellationToken cancellationToken)
    {
        using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            await dbContext.MawaqitPrayerTimes
                .Where(p => p.Date < deleteBeforeDate)
                .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
