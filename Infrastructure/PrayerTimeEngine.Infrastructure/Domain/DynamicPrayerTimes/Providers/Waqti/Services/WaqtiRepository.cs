using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Services;

public class WaqtiRepository(
        IDbContextFactory<AppDbContext> dbContextFactory
    ) : IWaqtiRepository, IPrayerTimeCacheCleaner
{
    private static readonly Func<AppDbContext, LocalDate, decimal, decimal, string, double?, double?, IAsyncEnumerable<WaqtiDailyPrayerTimes>> s_compiledQuery_GetPrayerTimesAsync =
        EF.CompileAsyncQuery(
            (AppDbContext context, LocalDate date, decimal longitude, decimal latitude, string method, double? fajrAngle, double? ishaaAngle) =>
                context.WaqtiPrayerTimes.AsNoTracking()
                .Where(x =>
                    x.Date == date
                    && x.Longitude == longitude
                    && x.Latitude == latitude
                    && x.Method == method
                    && x.FajrAngle == fajrAngle
                    && x.IshaaAngle == ishaaAngle));

    public async Task<WaqtiDailyPrayerTimes?> GetPrayerTimesAsync(
        LocalDate date,
        decimal longitude,
        decimal latitude,
        string method,
        double? fajrAngle,
        double? ishaaAngle,
        CancellationToken cancellationToken)
    {
        using AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await s_compiledQuery_GetPrayerTimesAsync(dbContext, date, longitude, latitude, method, fajrAngle, ishaaAngle)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task InsertPrayerTimesAsync(IEnumerable<WaqtiDailyPrayerTimes> waqtiPrayerTimesLst, CancellationToken cancellationToken)
    {
        List<WaqtiDailyPrayerTimes> toBeInserted = [.. waqtiPrayerTimesLst];

        if (toBeInserted.Count == 0)
        {
            return;
        }

        using AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // a month may be partly stored already (e.g. after the cache cleaner removed its past days)
        var alreadyStored = new HashSet<WaqtiDailyPrayerTimes>();
        foreach (WaqtiDailyPrayerTimes prayerTimes in toBeInserted)
        {
            if (await s_compiledQuery_GetPrayerTimesAsync(dbContext, prayerTimes.Date, prayerTimes.Longitude, prayerTimes.Latitude, prayerTimes.Method, prayerTimes.FajrAngle, prayerTimes.IshaaAngle)
                .AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                alreadyStored.Add(prayerTimes);
            }
        }

        await dbContext.WaqtiPrayerTimes
            .AddRangeAsync(toBeInserted.Where(x => !alreadyStored.Contains(x)), cancellationToken)
            .ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteCacheDataAsync(LocalDate deleteBeforeDate, CancellationToken cancellationToken)
    {
        using AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.WaqtiPrayerTimes
            .Where(p => p.Date < deleteBeforeDate)
            .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
    }
}
