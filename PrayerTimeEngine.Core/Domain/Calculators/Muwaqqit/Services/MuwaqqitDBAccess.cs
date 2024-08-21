using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services
{
    public class MuwaqqitDBAccess(
            IDbContextFactory<AppDbContext> dbContextFactory
        ) : IMuwaqqitDBAccess, IPrayerTimeCacheCleaner
    {
        public async Task<List<MuwaqqitPrayerTimes>> GetAllTimes(CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await dbContext
                    .MuwaqqitPrayerTimes.AsNoTracking()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private static readonly Func<AppDbContext, ZonedDateTime, decimal, decimal, double, double, double, double, IAsyncEnumerable<MuwaqqitPrayerTimes>> compiledQuery_GetTimesAsync =
            EF.CompileAsyncQuery(
                (AppDbContext context, ZonedDateTime date, decimal longitude, decimal latitude, double fajrDegree, double ishaDegree, double ishtibaqDegree, double asrKarahaDegree) =>
                    context.MuwaqqitPrayerTimes.AsNoTracking()
                    .Where(x =>
                        x.Date == date
                        && x.Longitude == longitude
                        && x.Latitude == latitude
                        && x.FajrDegree == fajrDegree
                        && x.IshaDegree == ishaDegree
                        && x.IshtibaqDegree == ishtibaqDegree
                        && x.AsrKarahaDegree == asrKarahaDegree));

        public async Task<MuwaqqitPrayerTimes> GetTimesAsync(
            ZonedDateTime date,
            decimal longitude,
            decimal latitude,
            double fajrDegree,
            double ishaDegree,
            double ishtibaqDegree,
            double asrKarahaDegree,
            CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                return await compiledQuery_GetTimesAsync(dbContext, date, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task InsertMuwaqqitPrayerTimesAsync(IEnumerable<MuwaqqitPrayerTimes> muwaqqitPrayerTimesLst, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                await dbContext.MuwaqqitPrayerTimes.AddRangeAsync(muwaqqitPrayerTimesLst, cancellationToken).ConfigureAwait(false);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task DeleteCacheDataAsync(ZonedDateTime deleteBeforeDate, CancellationToken cancellationToken)
        {
            using (AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                await dbContext.MuwaqqitPrayerTimes
                    .Where(p => p.Date.ToInstant() < deleteBeforeDate.ToInstant())
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
