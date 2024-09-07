using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Services;

public class MawaqitDBAccess(
        IDbContextFactory<AppDbContext> dbContextFactory
    ) : IMawaqitDBAccess, IPrayerTimeCacheCleaner
{
    private static readonly Func<AppDbContext, LocalDate, string, IAsyncEnumerable<MawaqitPrayerTimes>> compiledQuery_GetPrayerTimesAsync =
        EF.CompileAsyncQuery(
            (AppDbContext context, LocalDate date, string externalID) =>
                context.MawaqitPrayerTimes.AsNoTracking()
                .Where(x => x.Date == date && x.ExternalID == externalID));

    public async Task<MawaqitPrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken)
    {
        using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            return await compiledQuery_GetPrayerTimesAsync(dbContext, date, externalID)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task InsertPrayerTimesAsync(List<MawaqitPrayerTimes> prayerTimesLst, CancellationToken cancellationToken)
    {
        using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            await dbContext.MawaqitPrayerTimes.AddRangeAsync(prayerTimesLst, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task DeleteCacheDataAsync(ZonedDateTime deleteBeforeDate, CancellationToken cancellationToken)
    {
        using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            await dbContext.MawaqitPrayerTimes
                .Where(p => p.Date < deleteBeforeDate.LocalDateTime.Date)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
